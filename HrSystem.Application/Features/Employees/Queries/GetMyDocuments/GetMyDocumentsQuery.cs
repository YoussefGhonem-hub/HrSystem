using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storage.AWS3.Extensions;

namespace HrSystem.Application.Features.Employees.Queries.GetMyDocuments;

/// <summary>
/// Query to get grouped documents for the currently logged-in user
/// </summary>
public record GetMyDocumentsQuery : IRequest<ErrorOr<GenericResponse<List<DocumentGroupDto>>>>;

public class GetMyDocumentsQueryHandler : IRequestHandler<GetMyDocumentsQuery, ErrorOr<GenericResponse<List<DocumentGroupDto>>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public GetMyDocumentsQueryHandler(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ErrorOr<GenericResponse<List<DocumentGroupDto>>>> Handle(
        GetMyDocumentsQuery request,
        CancellationToken cancellationToken)
    {
        Guid? employeeId = CurrentUser.EmployeeId;

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            var userId = CurrentUser.Id;
            if (userId.HasValue)
            {
                employeeId = await _context.Employees
                    .Where(e => e.UserId == userId)
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (!employeeId.HasValue || employeeId.Value == Guid.Empty)
        {
            return Error.Unauthorized("Documents.Unauthorized", "Current user is not linked to an employee");
        }

        var documents = await _context.EmployeeDocuments
            .Include(d => d.DocumentType)
            .Where(d => !d.IsDeleted && d.EmployeeId == employeeId.Value)
            .OrderBy(d => d.DocumentType.DisplayOrder)
            .ThenBy(d => d.DocumentName)
            .Select(d => new DocumentItemDto
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
                    ? DocumentCategoryHelper.ResolveCategory(d.DocumentType.NameEn, d.DocumentName)
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
            .Select(g => new DocumentGroupDto
            {
                CategoryKey = g.Key,
                CategoryNameEn = DocumentCategoryHelper.GetCategoryNameEn(g.Key),
                CategoryNameAr = DocumentCategoryHelper.GetCategoryNameAr(g.Key),
                Items = g.ToList()
            })
            .OrderBy(g => g.CategoryKey)
            .ToList();

        return new GenericResponse<List<DocumentGroupDto>>
        {
            Success = true,
            Message = "Documents retrieved successfully",
            Data = groups
        };
    }

}
