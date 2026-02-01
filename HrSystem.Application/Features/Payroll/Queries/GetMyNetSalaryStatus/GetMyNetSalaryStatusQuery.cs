using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMyNetSalaryStatus;

/// <summary>
/// Returns only the net salary and paid status for the current (or requested) month.
/// </summary>
public record GetMyNetSalaryStatusQuery(int? Year = null, int? Month = null)
    : IRequest<ErrorOr<GenericResponse<MyNetSalaryStatusDto>>>;

public class GetMyNetSalaryStatusQueryHandler : IRequestHandler<GetMyNetSalaryStatusQuery, ErrorOr<GenericResponse<MyNetSalaryStatusDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMyNetSalaryStatusQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MyNetSalaryStatusDto>>> Handle(
        GetMyNetSalaryStatusQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var year = request.Year ?? now.Year;
        var month = request.Month ?? now.Month;

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
            return Error.Unauthorized("Payroll.Unauthorized", "Current user is not linked to an employee");
        }

        // Try to get the payslip for the requested/current month
        var payslip = await _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => p.EmployeeId == employeeId.Value &&
                        p.PayrollCycle.Year == year &&
                        p.PayrollCycle.Month == month)
            .OrderByDescending(p => p.GeneratedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (payslip != null)
        {
            var dto = new MyNetSalaryStatusDto
            {
                Year = year,
                Month = month,
                NetSalary = payslip.NetSalary,
                IsPaid = payslip.IsPaid
            };

            return new GenericResponse<MyNetSalaryStatusDto>
            {
                Success = true,
                Message = "Net salary status retrieved successfully",
                Data = dto
            };
        }

        // Fallback: use configured current salary (approximate net = basic + allowances - deductions)
        var salary = await _context.Salaries
            .Include(s => s.Allowances)
            .Include(s => s.Deductions)
            .Where(s => s.EmployeeId == employeeId.Value && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (salary == null)
        {
            return Error.NotFound("Payroll.NoSalary", "No salary configuration found for the current user");
        }

        decimal allowanceTotal = 0m;
        foreach (var a in salary.Allowances)
        {
            allowanceTotal += a.IsPercentage && a.PercentageValue.HasValue
                ? salary.BasicSalary * (a.PercentageValue.Value / 100m)
                : a.Amount;
        }

        decimal deductionTotal = 0m;
        foreach (var d in salary.Deductions)
        {
            deductionTotal += d.IsPercentage && d.PercentageValue.HasValue
                ? salary.BasicSalary * (d.PercentageValue.Value / 100m)
                : d.Amount;
        }

        var approximateNet = (salary.BasicSalary + allowanceTotal) - deductionTotal;

        var fallbackDto = new MyNetSalaryStatusDto
        {
            Year = year,
            Month = month,
            NetSalary = Math.Round(approximateNet, 2),
            IsPaid = false
        };

        return new GenericResponse<MyNetSalaryStatusDto>
        {
            Success = true,
            Message = "Net salary approximated from current configuration (no payslip found)",
            Data = fallbackDto
        };
    }
}
