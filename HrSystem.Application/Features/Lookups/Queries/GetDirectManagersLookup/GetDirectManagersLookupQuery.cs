using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Features.Lookups.Queries.GetEmployeesLookup;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
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
        // Apply tenant filtering (organization scope)
        var query = _context.Employees
            .WhereNotDeleted()
            .Where(e => e.TenantId == CurrentUser.OrganizationId);

        // Apply branch filtering for HR managers (SuperAdmin and OrgAdmin see all)
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        
        if (!isSuperOrOrgAdmin && CurrentUser.BranchId.HasValue)
        {
            query = query.Where(e => e.BranchId == CurrentUser.BranchId.Value);
        }

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
