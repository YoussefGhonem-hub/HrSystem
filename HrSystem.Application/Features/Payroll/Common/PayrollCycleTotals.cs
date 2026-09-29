using HrSystem.Domain.Entities.Payroll;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Common;

/// <summary>
/// Recomputes a payroll cycle's aggregate totals from its live (non-deleted) payslips.
/// Used after generating, updating or deleting payslips so the totals never drift.
/// </summary>
public static class PayrollCycleTotals
{
    public static async Task RecalculateAsync(
        ApplicationDbContext context,
        PayrollCycle cycle,
        CancellationToken cancellationToken)
    {
        var totals = await context.Payslips
            .IgnoreQueryFilters()
            .Where(p => p.PayrollCycleId == cycle.Id && !p.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Gross = g.Sum(p => p.GrossSalary),
                Net = g.Sum(p => p.NetSalary),
                Deductions = g.Sum(p => p.TotalDeductions),
                Tax = g.Sum(p => p.IncomeTax),
                Insurance = g.Sum(p => p.SocialInsuranceEmployee),
                AllPaid = g.All(p => p.IsPaid)
            })
            .FirstOrDefaultAsync(cancellationToken);

        cycle.TotalGrossSalary = totals?.Gross ?? 0m;
        cycle.TotalNetSalary = totals?.Net ?? 0m;
        cycle.TotalDeductions = totals?.Deductions ?? 0m;
        cycle.TotalTax = totals?.Tax ?? 0m;
        cycle.TotalInsurance = totals?.Insurance ?? 0m;

        if (totals is null || totals.Count == 0)
        {
            cycle.StatusId = PayrollStatusIds.Draft;
            cycle.PaymentDate = null;
        }
        else if (totals.AllPaid)
        {
            cycle.StatusId = PayrollStatusIds.Paid;
        }
        else if (cycle.StatusId == PayrollStatusIds.Paid)
        {
            // Some payslips were regenerated/added after payment: the cycle is no longer fully paid.
            cycle.StatusId = PayrollStatusIds.Processed;
        }
    }
}
