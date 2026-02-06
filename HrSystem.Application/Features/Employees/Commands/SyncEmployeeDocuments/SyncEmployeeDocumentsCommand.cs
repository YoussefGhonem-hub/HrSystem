using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetMyDocuments;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Commands.SyncEmployeeDocuments;

/// <summary>
/// Represents a single document in the sync request
/// - If DocumentId is provided: update existing document
/// - If DocumentId is null/empty: create new document
/// </summary>
public class SyncDocumentItem
{
    /// <summary>
    /// Document ID - if provided, update existing; if null/empty, create new
    /// </summary>
    public Guid? DocumentId { get; set; }

    /// <summary>
    /// Document type (required for new documents)
    /// </summary>
    public EmployeeDocumentType DocumentType { get; set; }

    /// <summary>
    /// Document name
    /// </summary>
    public string? DocumentName { get; set; }

    /// <summary>
    /// Document description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Document expiry date
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// File to upload (required for new documents, optional for updates)
    /// </summary>
    public IFormFile? File { get; set; }
}

/// <summary>
/// Command to sync employee documents:
/// - Update existing documents (with DocumentId)
/// - Create new documents (without DocumentId)
/// - Delete documents not included in the request
/// </summary>
public class SyncEmployeeDocumentsCommand : IRequest<ErrorOr<GenericResponse<List<DocumentItemDto>>>>
{
    /// <summary>
    /// The employee ID whose documents are being synced
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// List of documents to sync
    /// </summary>
    public List<SyncDocumentItem> Documents { get; set; } = new();
}

public class SyncEmployeeDocumentsCommandHandler : IRequestHandler<SyncEmployeeDocumentsCommand, ErrorOr<GenericResponse<List<DocumentItemDto>>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public SyncEmployeeDocumentsCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<List<DocumentItemDto>>>> Handle(
        SyncEmployeeDocumentsCommand request,
        CancellationToken cancellationToken)
    {
        // Find employee
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && !e.IsDeleted, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        // Authorization check
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to manage documents for this employee");
            }
        }

        // Get all existing documents for this employee
        var existingDocuments = await _context.EmployeeDocuments
            .Where(d => d.EmployeeId == request.EmployeeId && !d.IsDeleted)
            .ToListAsync(cancellationToken);

        // Get document IDs from request (only those with valid IDs)
        var requestDocumentIds = request.Documents
            .Where(d => d.DocumentId.HasValue && d.DocumentId.Value != Guid.Empty)
            .Select(d => d.DocumentId!.Value)
            .ToHashSet();

        // Find documents to delete (exist in DB but not in request)
        var documentsToDelete = existingDocuments
            .Where(d => !requestDocumentIds.Contains(d.Id))
            .ToList();

        // Delete old files from S3 and soft delete documents
        foreach (var docToDelete in documentsToDelete)
        {
            // Delete file from S3
            if (!string.IsNullOrWhiteSpace(docToDelete.FilePath))
            {
                await _storageService.Delete(docToDelete.FilePath, cancellationToken);
            }

            // Soft delete
            docToDelete.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
        }

        var resultDocuments = new List<DocumentItemDto>();

        // Process each document in the request
        foreach (var item in request.Documents)
        {
            if (item.DocumentId.HasValue && item.DocumentId.Value != Guid.Empty)
            {
                // UPDATE existing document
                var existingDoc = existingDocuments.FirstOrDefault(d => d.Id == item.DocumentId.Value);
                
                if (existingDoc == null)
                {
                    // Document not found, skip or return error
                    continue;
                }

                // Update fields
                existingDoc.DocumentType = item.DocumentType;
                
                if (!string.IsNullOrWhiteSpace(item.DocumentName))
                {
                    existingDoc.DocumentName = item.DocumentName;
                }

                existingDoc.Description = item.Description;
                existingDoc.ExpiryDate = item.ExpiryDate;

                // Handle file replacement if new file uploaded
                if (item.File != null && item.File.Length > 0)
                {
                    var oldFilePath = existingDoc.FilePath;

                    // Upload new file to S3
                    var stored = await _storageService.Upload(item.File, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(stored.Key))
                    {
                        var fileUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);

                        existingDoc.FilePath = stored.Key;
                        existingDoc.FileUrl = fileUrl;
                        existingDoc.FileSize = item.File.Length;
                        existingDoc.ContentType = item.File.ContentType;

                        // Delete old file from S3
                        if (!string.IsNullOrWhiteSpace(oldFilePath))
                        {
                            await _storageService.Delete(oldFilePath, cancellationToken);
                        }
                    }
                }

                existingDoc.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

                var typeInfo = existingDoc.DocumentType.GetInfo();
                resultDocuments.Add(new DocumentItemDto
                {
                    Id = existingDoc.Id,
                    DocumentType = existingDoc.DocumentType,
                    DocumentTypeNameEn = typeInfo.NameEn,
                    DocumentTypeNameAr = typeInfo.NameAr,
                    DocumentName = existingDoc.DocumentName,
                    FileUrl = existingDoc.FileUrl,
                    FilePath = existingDoc.FilePath,
                    ContentType = existingDoc.ContentType,
                    FileSize = existingDoc.FileSize,
                    ExpiryDate = existingDoc.ExpiryDate,
                    Description = existingDoc.Description,
                    Category = typeInfo.Category
                });
            }
            else
            {
                // CREATE new document
                if (item.File == null || item.File.Length == 0)
                {
                    // Skip new documents without file
                    continue;
                }

                // Upload file to S3
                var stored = await _storageService.Upload(item.File, cancellationToken);
                if (string.IsNullOrWhiteSpace(stored.Key))
                {
                    continue; // Skip if upload failed
                }

                var fileUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);
                var typeInfo = item.DocumentType.GetInfo();

                var newDocument = new EmployeeDocument
                {
                    EmployeeId = request.EmployeeId,
                    DocumentType = item.DocumentType,
                    DocumentName = string.IsNullOrWhiteSpace(item.DocumentName) ? typeInfo.NameEn : item.DocumentName,
                    FilePath = stored.Key,
                    FileUrl = fileUrl,
                    Description = item.Description,
                    ExpiryDate = item.ExpiryDate,
                    FileSize = item.File.Length,
                    ContentType = item.File.ContentType,
                    TenantId = employee.TenantId
                };

                newDocument.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);
                _context.EmployeeDocuments.Add(newDocument);

                resultDocuments.Add(new DocumentItemDto
                {
                    Id = newDocument.Id,
                    DocumentType = newDocument.DocumentType,
                    DocumentTypeNameEn = typeInfo.NameEn,
                    DocumentTypeNameAr = typeInfo.NameAr,
                    DocumentName = newDocument.DocumentName,
                    FileUrl = newDocument.FileUrl,
                    FilePath = newDocument.FilePath,
                    ContentType = newDocument.ContentType,
                    FileSize = newDocument.FileSize,
                    ExpiryDate = newDocument.ExpiryDate,
                    Description = newDocument.Description,
                    Category = typeInfo.Category
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<List<DocumentItemDto>>
        {
            Success = true,
            Message = $"Documents synced successfully. Created: {resultDocuments.Count(d => requestDocumentIds.Contains(d.Id) == false)}, Updated: {resultDocuments.Count(d => requestDocumentIds.Contains(d.Id))}, Deleted: {documentsToDelete.Count}",
            Data = resultDocuments
        };
    }
}
