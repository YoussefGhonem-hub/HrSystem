using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayslipsList;

public record GetPayslipsListQuery(
    Guid? EmployeeId = null,
    int? Year = null,
    int? Month = null,
    bool? IsPaid = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? SortBy = null,
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<PayslipListItemDto>>>>;

public class GetPayslipsListQueryHandler : IRequestHandler<GetPayslipsListQuery, ErrorOr<GenericResponse<PagedResult<PayslipListItemDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayslipsListQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PagedResult<PayslipListItemDto>>>> Handle(GetPayslipsListQuery request, CancellationToken cancellationToken)
    {
        var isPrivileged = CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.HRManager) == true
            || CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true;

        Guid? effectiveEmployeeId = request.EmployeeId;

        if (!isPrivileged)
        {
            effectiveEmployeeId = CurrentUser.EmployeeId;
            if (!effectiveEmployeeId.HasValue || effectiveEmployeeId.Value == Guid.Empty)
            {
                var userId = CurrentUser.Id;
                if (userId.HasValue)
                {
                    effectiveEmployeeId = await _context.Employees
                        .Where(e => e.UserId == userId)
                        .Select(e => e.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                }
            }

            if (!effectiveEmployeeId.HasValue || effectiveEmployeeId.Value == Guid.Empty)
            {
                return Error.Unauthorized("Payslip.Unauthorized", "Current user is not linked to an employee");
            }
        }

        // Apply branch scope for HR managers (not super/org admins)
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var query = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Include(p => p.Employee)
            .Where(p => !p.IsDeleted);

        if (branchId.HasValue && isPrivileged)
        {
            query = query.Where(p => p.Employee.BranchId == branchId.Value);
        }

        if (effectiveEmployeeId.HasValue)
        {
            query = query.Where(p => p.EmployeeId == effectiveEmployeeId.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(p => p.PayrollCycle.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(p => p.PayrollCycle.Month == request.Month.Value);
        }

        if (request.IsPaid.HasValue)
        {
            query = query.Where(p => p.IsPaid == request.IsPaid.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(p => p.GeneratedDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(p => p.GeneratedDate <= request.ToDate.Value);
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "employee" => request.SortDescending ? query.OrderByDescending(p => p.Employee.FullNameEn) : query.OrderBy(p => p.Employee.FullNameEn),
            "year" => request.SortDescending ? query.OrderByDescending(p => p.PayrollCycle.Year) : query.OrderBy(p => p.PayrollCycle.Year),
            "month" => request.SortDescending ? query.OrderByDescending(p => p.PayrollCycle.Month) : query.OrderBy(p => p.PayrollCycle.Month),
            "net" => request.SortDescending ? query.OrderByDescending(p => p.NetSalary) : query.OrderBy(p => p.NetSalary),
            "gross" => request.SortDescending ? query.OrderByDescending(p => p.GrossSalary) : query.OrderBy(p => p.GrossSalary),
            "deductions" => request.SortDescending ? query.OrderByDescending(p => p.TotalDeductions) : query.OrderBy(p => p.TotalDeductions),
            "generated" => request.SortDescending ? query.OrderByDescending(p => p.GeneratedDate) : query.OrderBy(p => p.GeneratedDate),
            "paid" => request.SortDescending ? query.OrderByDescending(p => p.PaidDate) : query.OrderBy(p => p.PaidDate),
            _ => query.OrderByDescending(p => p.PayrollCycle.Year).ThenByDescending(p => p.PayrollCycle.Month).ThenByDescending(p => p.GeneratedDate)
        };

        var projected = query.Select(p => new PayslipListItemDto
        {
            PayslipId = p.Id,
            EmployeeId = p.EmployeeId,
            EmployeeCode = p.Employee.EmployeeCode,
            EmployeeName = p.Employee.FullNameEn,
            Year = p.PayrollCycle.Year,
            Month = p.PayrollCycle.Month,
            GrossSalary = p.GrossSalary,
            TotalDeductions = p.TotalDeductions,
            NetSalary = p.NetSalary,
            IsPaid = p.IsPaid,
            GeneratedDate = p.GeneratedDate,
            PaidDate = p.PaidDate,
            PdfFileUrl = p.PdfFileUrl
        });

        var paged = await projected.ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return new GenericResponse<PagedResult<PayslipListItemDto>>
        {
            Success = true,
            Message = "Payslips history retrieved successfully",
            Data = paged
        };
    }
}
