using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Extensions;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDocuments;

/// <summary>
/// Query to get documents for a specific employee (HR or owner)
/// </summary>
public record GetEmployeeDocumentsQuery(Guid EmployeeId)
    : IRequest<ErrorOr<GenericResponse<List<Employees.Queries.GetMyDocuments.DocumentGroupDto>>>>;

public class GetEmployeeDocumentsQueryHandler : IRequestHandler<GetEmployeeDocumentsQuery, ErrorOr<GenericResponse<List<Employees.Queries.GetMyDocuments.DocumentGroupDto>>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public GetEmployeeDocumentsQueryHandler(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ErrorOr<GenericResponse<List<Employees.Queries.GetMyDocuments.DocumentGroupDto>>>> Handle(
        GetEmployeeDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
               CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
               CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("Documents.Unauthorized", "Not allowed to access employee documents");
            }
        }

        var documents = await _context.EmployeeDocuments
            .Include(d => d.DocumentType)
            .Where(d => !d.IsDeleted && d.EmployeeId == request.EmployeeId)
            .OrderBy(d => d.DocumentType.DisplayOrder)
            .ThenBy(d => d.DocumentName)
                .Select(d => new Employees.Queries.GetMyDocuments.DocumentItemDto
            {
                Id = d.Id,
                DocumentTypeId = d.DocumentTypeId,
                DocumentTypeNameEn = d.DocumentType.NameEn,
                DocumentTypeNameAr = d.DocumentType.NameAr,
                DocumentName = d.DocumentName,
                FileUrl = d.FileUrl ?? d.FilePath,
                FilePath = d.FilePath,
                ContentType = d.ContentType,
                FileSize = d.FileSize,
                ExpiryDate = d.ExpiryDate,
                Description = d.Description,
                Category = d.DocumentType.CategoryKey == default
                    ? Employees.Queries.GetMyDocuments.DocumentCategoryHelper.ResolveCategory(d.DocumentType.NameEn, d.DocumentName)
                    : d.DocumentType.CategoryKey
            })
            .ToListAsync(cancellationToken);

        foreach (var document in documents)
        {
            if (!string.IsNullOrWhiteSpace(document.FilePath))
            {
                document.FileUrl = await _configuration.GetPreSignedUrlAsync(document.FilePath);
            }
        }

        var groups = documents
            .GroupBy(d => d.Category)
            .Select(g => new Employees.Queries.GetMyDocuments.DocumentGroupDto
            {
                CategoryKey = g.Key,
                CategoryNameEn = Employees.Queries.GetMyDocuments.DocumentCategoryHelper.GetCategoryNameEn(g.Key),
                CategoryNameAr = Employees.Queries.GetMyDocuments.DocumentCategoryHelper.GetCategoryNameAr(g.Key),
                Items = g.ToList()
            })
            .OrderBy(g => g.CategoryKey)
            .ToList();

        return new GenericResponse<List<Employees.Queries.GetMyDocuments.DocumentGroupDto>>
        {
            Success = true,
            Message = "Documents retrieved successfully",
            Data = groups
        };
    }
}
