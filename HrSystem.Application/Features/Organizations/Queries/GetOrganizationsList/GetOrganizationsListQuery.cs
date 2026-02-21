using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationsList;

public record GetOrganizationsListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null
) : IRequest<ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>>;

public record OrganizationListDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? DefaultLanguage { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsActive { get; init; }
    public int BranchesCount { get; init; }
    public int EmployeesCount { get; init; }
}

public class GetOrganizationsListQueryHandler : IRequestHandler<GetOrganizationsListQuery, ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationsListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<OrganizationListDto>>>> Handle(
        GetOrganizationsListQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Organizations
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(o =>
                o.NameEn.Contains(term) ||
                o.NameAr.Contains(term) ||
                o.Code.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(o => o.CreatedDate)
            .Skip(PagedResultExtensions.CalculateSkip(request.PageNumber, request.PageSize))
            .Take(PagedResultExtensions.CalculateTake(request.PageSize))
            .Select(o => new OrganizationListDto
            {
                Id = o.Id,
                NameEn = o.NameEn,
                NameAr = o.NameAr,
                Code = o.Code,
                Industry = o.Industry,
                DefaultLanguage = o.DefaultLanguage,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                IsActive = o.IsActive,
                BranchesCount = o.Branches.Count,
                EmployeesCount = _context.Employees.Count(e => e.TenantId == o.Id && !e.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        var paged = PagedResult<OrganizationListDto>.Create(items, totalCount, request.PageNumber, request.PageSize);

        return new GenericResponse<PagedResult<OrganizationListDto>>
        {
            Success = true,
            Data = paged
        };
    }
}
