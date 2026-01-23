using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;

/// <summary>
/// Query to get gross/net salary for the currently logged-in user
/// </summary>
public record GetMySalarySummaryQuery(int? Year = null, int? Month = null) : IRequest<ErrorOr<GenericResponse<MySalarySummaryDto>>>;

public class GetMySalarySummaryQueryHandler : IRequestHandler<GetMySalarySummaryQuery, ErrorOr<GenericResponse<MySalarySummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMySalarySummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<MySalarySummaryDto>>> Handle(
        GetMySalarySummaryQuery request,
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
            return Error.Unauthorized("Payroll.Unauthorized", "Current user is not linked to an employee");
        }

        var payslipQuery = _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => p.EmployeeId == employeeId.Value)
            .AsQueryable();

        if (request.Year.HasValue)
        {
            payslipQuery = payslipQuery.Where(p => p.PayrollCycle.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            payslipQuery = payslipQuery.Where(p => p.PayrollCycle.Month == request.Month.Value);
        }

        var summary = await payslipQuery
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .Select(p => new MySalarySummaryDto
            {
                PayslipId = p.Id,
                PayrollCycleId = p.PayrollCycleId,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                GrossSalary = p.GrossSalary,
                NetSalary = p.NetSalary,
                GeneratedDate = p.GeneratedDate,
                IsPaid = p.IsPaid,
                PaidDate = p.PaidDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (summary == null)
        {
            return Error.NotFound("Payroll.NoPayslip", "No payslip found for the current user");
        }

        return new GenericResponse<MySalarySummaryDto>
        {
            Success = true,
            Message = "Salary summary retrieved successfully",
            Data = summary
        };
    }
}
