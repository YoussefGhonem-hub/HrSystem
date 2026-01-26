using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetPayrollSummary;

public record GetPayrollSummaryQuery(int? Year = null, int? Month = null) : IRequest<ErrorOr<GenericResponse<PayrollSummaryDto>>>;

public class GetPayrollSummaryQueryHandler : IRequestHandler<GetPayrollSummaryQuery, ErrorOr<GenericResponse<PayrollSummaryDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPayrollSummaryQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<PayrollSummaryDto>>> Handle(GetPayrollSummaryQuery request, CancellationToken cancellationToken)
    {
        var payslipQuery = _context.Payslips
            .Include(p => p.PayrollCycle)
            .AsQueryable();

        if (request.Year.HasValue)
        {
            payslipQuery = payslipQuery.Where(p => p.PayrollCycle.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            payslipQuery = payslipQuery.Where(p => p.PayrollCycle.Month == request.Month.Value);
        }

        // If no filters provided, restrict to latest available cycle within scope
        if (!request.Year.HasValue && !request.Month.HasValue)
        {
            var latestCycle = await _context.PayrollCycles
                .OrderByDescending(c => c.Year)
                .ThenByDescending(c => c.Month)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestCycle == null)
            {
                return Error.NotFound("Payroll.NoCycles", "No payroll cycles found for the current scope");
            }

            payslipQuery = payslipQuery.Where(p => p.PayrollCycleId == latestCycle.Id);
            request = request with { Year = latestCycle.Year, Month = latestCycle.Month };
        }

        // Compute metrics
        var employeesPaid = await payslipQuery
            .Where(p => p.IsPaid)
            .Select(p => p.EmployeeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var gross = await payslipQuery.SumAsync(p => (decimal?)p.GrossSalary, cancellationToken) ?? 0m;
        var deductions = await payslipQuery.SumAsync(p => (decimal?)p.TotalDeductions, cancellationToken) ?? 0m;
        var net = await payslipQuery.SumAsync(p => (decimal?)p.NetSalary, cancellationToken) ?? 0m;

        var dto = new PayrollSummaryDto
        {
            EmployeesPaid = employeesPaid,
            Gross = gross,
            Deductions = deductions,
            NetPayroll = net,
            Year = request.Year,
            Month = request.Month
        };

        return new GenericResponse<PayrollSummaryDto>
        {
            Success = true,
            Message = "Payroll summary retrieved successfully",
            Data = dto
        };
    }
}
