using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetMyDocuments;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployeeDocument;

/// <summary>
/// Command to update an existing employee document with optional file replacement
/// </summary>
public record UpdateEmployeeDocumentCommand(
    Guid DocumentId,
    EmployeeDocumentType? DocumentType,
    string? DocumentName,
    string? Description,
    DateTime? ExpiryDate,
    IFormFile? File
) : IRequest<ErrorOr<GenericResponse<DocumentItemDto>>>;

public class UpdateEmployeeDocumentCommandHandler : IRequestHandler<UpdateEmployeeDocumentCommand, ErrorOr<GenericResponse<DocumentItemDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public UpdateEmployeeDocumentCommandHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<DocumentItemDto>>> Handle(
        UpdateEmployeeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        // Find the existing document
        var document = await _context.EmployeeDocuments
            .Include(d => d.Employee)
            .FirstOrDefaultAsync(d => d.Id == request.DocumentId && !d.IsDeleted, cancellationToken);

        if (document == null)
        {
            return Error.NotFound("Document.NotFound", "Employee document not found");
        }

        // Authorization check - HR roles or document owner
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != document.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to update this document");
            }
        }

        // Track if we need to delete old file from S3
        string? oldFilePath = null;

        // Update document type if provided
        if (request.DocumentType.HasValue)
        {
            document.DocumentType = request.DocumentType.Value;
        }

        // Update document name if provided
        if (!string.IsNullOrWhiteSpace(request.DocumentName))
        {
            document.DocumentName = request.DocumentName;
        }

        // Update description (can be set to null)
        document.Description = request.Description ?? document.Description;

        // Update expiry date if provided
        if (request.ExpiryDate.HasValue)
        {
            document.ExpiryDate = request.ExpiryDate;
        }

        // Handle file replacement if a new file is uploaded
        if (request.File != null && request.File.Length > 0)
        {
            // Store old file path for deletion after successful upload
            oldFilePath = document.FilePath;

            // Upload new file to Amazon S3
            var stored = await _storageService.Upload(request.File, cancellationToken);
            if (string.IsNullOrWhiteSpace(stored.Key))
            {
                return Error.Failure("Documents.UploadFailed", "Failed to upload new file");
            }

            // Generate presigned URL from S3
            var fileUrl = await _storageService.DownloadFileUrl(stored.Key, cancellationToken);

            // Update document with new file details
            document.FilePath = stored.Key;
            document.FileUrl = fileUrl;
            document.FileSize = request.File.Length;
            document.ContentType = request.File.ContentType;

            // Delete old file from S3 after successful upload
            if (!string.IsNullOrWhiteSpace(oldFilePath))
            {
                await _storageService.Delete(oldFilePath, cancellationToken);
            }
        }

        // Mark as modified
        document.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        // Get type info for response
        var typeInfo = document.DocumentType.GetInfo();

        var dto = new DocumentItemDto
        {
            Id = document.Id,
            DocumentType = document.DocumentType,
            DocumentTypeNameEn = typeInfo.NameEn,
            DocumentTypeNameAr = typeInfo.NameAr,
            DocumentName = document.DocumentName,
            FileUrl = document.FileUrl,
            FilePath = document.FilePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            ExpiryDate = document.ExpiryDate,
            Description = document.Description,
            Category = typeInfo.Category
        };

        return new GenericResponse<DocumentItemDto>
        {
            Success = true,
            Message = "Document updated successfully",
            Data = dto
        };
    }
}
