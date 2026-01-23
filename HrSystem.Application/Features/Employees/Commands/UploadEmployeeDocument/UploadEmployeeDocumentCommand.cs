using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetMyDocuments;
using HrSystem.Domain.Entities.Employee;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Extensions;

namespace HrSystem.Application.Features.Employees.Commands.UploadEmployeeDocument;

public record UploadEmployeeDocumentCommand(
    Guid EmployeeId,
    Guid DocumentTypeId,
    string? DocumentName,
    string? Description,
    DateTime? ExpiryDate,
    IFormFile File
) : IRequest<ErrorOr<GenericResponse<DocumentItemDto>>>;

public class UploadEmployeeDocumentCommandHandler : IRequestHandler<UploadEmployeeDocumentCommand, ErrorOr<GenericResponse<DocumentItemDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public UploadEmployeeDocumentCommandHandler(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ErrorOr<GenericResponse<DocumentItemDto>>> Handle(
        UploadEmployeeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
               CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
               CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
               CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to upload document");
            }
        }

        var documentType = await _context.EmployeeDocumentTypes
            .FirstOrDefaultAsync(dt => dt.Id == request.DocumentTypeId && dt.IsActive, cancellationToken);

        if (documentType == null)
        {
            return Error.NotFound("Documents.TypeNotFound", "Document type not found");
        }

        var stored = await _configuration.UploadToS3Async(request.File, cancellationToken);
        if (string.IsNullOrWhiteSpace(stored.Key))
        {
            return Error.Failure("Documents.UploadFailed", "File upload failed");
        }

        var document = new EmployeeDocument
        {
            EmployeeId = request.EmployeeId,
            DocumentTypeId = documentType.Id,
            DocumentName = string.IsNullOrWhiteSpace(request.DocumentName) ? documentType.NameEn : request.DocumentName!,
            FilePath = stored.Key,
            FileUrl = await _configuration.GetPreSignedUrlAsync(stored.Key),
            Description = request.Description,
            ExpiryDate = request.ExpiryDate,
            FileSize = request.File.Length,
            ContentType = request.File.ContentType,
            TenantId = employee.TenantId
        };

        document.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new DocumentItemDto
        {
            Id = document.Id,
            DocumentTypeId = document.DocumentTypeId,
            DocumentTypeNameEn = documentType.NameEn,
            DocumentTypeNameAr = documentType.NameAr,
            DocumentName = document.DocumentName,
            FileUrl = document.FileUrl,
            FilePath = document.FilePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            ExpiryDate = document.ExpiryDate,
            Description = document.Description,
            Category = documentType.CategoryKey == default
                ? DocumentCategoryHelper.ResolveCategory(documentType.NameEn, document.DocumentName)
                : documentType.CategoryKey
        };

        return new GenericResponse<DocumentItemDto>
        {
            Success = true,
            Message = "Document uploaded successfully",
            Data = dto
        };
    }
}
