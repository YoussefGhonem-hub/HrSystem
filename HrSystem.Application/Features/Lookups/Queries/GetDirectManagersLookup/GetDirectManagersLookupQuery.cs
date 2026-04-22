using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Features.Lookups.Queries.GetEmployeesLookup;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetDirectManagersLookup;

public record GetDirectManagersLookupQuery() : IRequest<ErrorOr<GenericResponse<List<EmployeeLookupDto>>>>;

public class GetDirectManagersLookupQueryHandler : IRequestHandler<GetDirectManagersLookupQuery, ErrorOr<GenericResponse<List<EmployeeLookupDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetDirectManagersLookupQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<EmployeeLookupDto>>>> Handle(
        GetDirectManagersLookupQuery request,
        CancellationToken cancellationToken)
    {
        // Start with base query — all non-deleted employees in the same organization
        var query = _context.Employees
            .Include(e => e.Department)
            .WhereNotDeleted();

        // Apply tenant filtering (organization scope) if OrganizationId is set
        if (CurrentUser.OrganizationId.HasValue)
        {
            query = query.Where(e => e.TenantId == CurrentUser.OrganizationId.Value);
        }

        // NOTE: Branch filtering is intentionally NOT applied here.
        // A direct manager can belong to any branch within the same organization.

        var directManagers = await query
            .OrderBy(e => e.FirstNameEn)
            .ThenBy(e => e.LastNameEn)
            .Select(e => new EmployeeLookupDto
            {
                Id = e.Id,
                FullNameEn = e.FirstNameEn + " " + e.LastNameEn,
                FullNameAr = e.FirstNameAr + " " + e.LastNameAr,
                EmployeeCode = e.EmployeeCode,
                DepartmentNameEn = e.Department != null ? e.Department.NameEn : null,
                DepartmentNameAr = e.Department != null ? e.Department.NameAr : null
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<EmployeeLookupDto>>
        {
            Success = true,
            Message = "Direct managers retrieved successfully",
            Data = directManagers
        };
    }
}
