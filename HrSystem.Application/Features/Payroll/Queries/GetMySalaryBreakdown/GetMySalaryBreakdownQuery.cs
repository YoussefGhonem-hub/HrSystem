using ErrorOr;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetMySalaryBreakdown;

/// <summary>
/// Provides a detailed salary breakdown for the current (or requested) month
/// including earnings items, deductions items, totals, and paid status.
/// </summary>
public record GetMySalaryBreakdownQuery(int? Year = null, int? Month = null)
    : IRequest<ErrorOr<GenericResponse<SalaryBreakdownDto>>>;

public class GetMySalaryBreakdownQueryHandler : IRequestHandler<GetMySalaryBreakdownQuery, ErrorOr<GenericResponse<SalaryBreakdownDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMySalaryBreakdownQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<SalaryBreakdownDto>>> Handle(
        GetMySalaryBreakdownQuery request,
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
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => !p.IsDeleted && p.EmployeeId == employeeId.Value &&
                        p.PayrollCycle.Year == year && p.PayrollCycle.Month == month)
            .OrderByDescending(p => p.GeneratedDate)
            .FirstOrDefaultAsync(cancellationToken);

        var breakdown = new SalaryBreakdownDto
        {
            Year = year,
            Month = month,
            PeriodLabel = new DateTime(year, month, 1).ToString("MMMM yyyy")
        };

        if (payslip != null)
        {
            // Earnings: Basic + Payslip allowances + Overtime + Bonus
            breakdown.Earnings.Add(new BreakdownItemDto { Name = "Basic Salary", Amount = payslip.BasicSalary });

            foreach (var a in payslip.PayslipAllowances)
            {
                breakdown.Earnings.Add(new BreakdownItemDto
                {
                    Name = string.IsNullOrWhiteSpace(a.AllowanceNameEn) ? a.AllowanceNameAr : a.AllowanceNameEn,
                    Amount = a.Amount
                });
            }

            if (payslip.OvertimeAmount > 0)
                breakdown.Earnings.Add(new BreakdownItemDto { Name = "Overtime", Amount = payslip.OvertimeAmount });
            if (payslip.BonusAmount > 0)
                breakdown.Earnings.Add(new BreakdownItemDto { Name = "Bonus", Amount = payslip.BonusAmount });

            // Deductions: Income Tax, Social Insurance (employee), Leave Deductions, plus listed payslip deductions
            if (payslip.IncomeTax > 0)
                breakdown.Deductions.Add(new BreakdownItemDto { Name = "Income Tax", Amount = payslip.IncomeTax });

            if (payslip.SocialInsuranceEmployee > 0)
                breakdown.Deductions.Add(new BreakdownItemDto { Name = "Social Insurance", Amount = payslip.SocialInsuranceEmployee });

            if (payslip.LeaveDeductions > 0)
                breakdown.Deductions.Add(new BreakdownItemDto { Name = "Leave Deduction", Amount = payslip.LeaveDeductions });

            foreach (var d in payslip.PayslipDeductions)
            {
                breakdown.Deductions.Add(new BreakdownItemDto
                {
                    Name = string.IsNullOrWhiteSpace(d.DeductionNameEn) ? d.DeductionNameAr : d.DeductionNameEn,
                    Amount = d.Amount
                });
            }

            breakdown.TotalEarnings = payslip.GrossSalary;
            breakdown.TotalDeductions = payslip.TotalDeductions;
            breakdown.NetSalary = payslip.NetSalary;
            breakdown.IsPaid = payslip.IsPaid;

            return new GenericResponse<SalaryBreakdownDto>
            {
                Success = true,
                Message = "Salary breakdown retrieved successfully",
                Data = breakdown
            };
        }

        // Fallback: derive from current salary config
        var salary = await _context.Salaries
            .Include(s => s.Allowances)
            .Include(s => s.Deductions)
            .Where(s => s.EmployeeId == employeeId.Value && s.IsCurrent)
            .OrderByDescending(s => s.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (salary is null)
        {
            return Error.NotFound("Payroll.NoSalary", "No salary configuration found for the current user");
        }

        breakdown.Earnings.Add(new BreakdownItemDto { Name = "Basic Salary", Amount = salary.BasicSalary });

        decimal allowanceTotal = 0m;
        foreach (var a in salary.Allowances)
        {
            var amount = a.IsPercentage && a.PercentageValue.HasValue
                ? salary.BasicSalary * (a.PercentageValue.Value / 100m)
                : a.Amount;
            allowanceTotal += amount;
            breakdown.Earnings.Add(new BreakdownItemDto
            {
                Name = string.IsNullOrWhiteSpace(a.NameEn) ? "Allowance" : a.NameEn,
                Amount = Math.Round(amount, 2)
            });
        }

        decimal deductionTotal = 0m;
        foreach (var d in salary.Deductions)
        {
            var amount = d.IsPercentage && d.PercentageValue.HasValue
                ? salary.BasicSalary * (d.PercentageValue.Value / 100m)
                : d.Amount;
            deductionTotal += amount;
            breakdown.Deductions.Add(new BreakdownItemDto
            {
                Name = string.IsNullOrWhiteSpace(d.NameEn) ? "Deduction" : d.NameEn,
                Amount = Math.Round(amount, 2)
            });
        }

        // Social Insurance (approximate) if enabled
        if (salary.IsSocialInsuranceEnabled && salary.SocialInsuranceEmployeeRate.HasValue && salary.SocialInsuranceEmployeeRate.Value > 0)
        {
            var siAmount = salary.BasicSalary * (salary.SocialInsuranceEmployeeRate.Value / 100m);
            deductionTotal += siAmount;
            breakdown.Deductions.Add(new BreakdownItemDto { Name = "Social Insurance", Amount = Math.Round(siAmount, 2) });
        }

        // Overtime from approved requests for this month
        decimal overtimeAmount = 0m;
        var periodStart = new DateTime(year, month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var approvedOvertime = await _context.EmployeeRequests
            .Include(r => r.OvertimeDetail)
            .Include(r => r.RequestTypeRef)
            .Where(r => r.EmployeeId == employeeId.Value
                        && r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime"
                        && r.Status == EmployeeRequestStatus.Approved
                        && r.OvertimeDetail != null
                        && r.OvertimeDetail.OvertimeDate >= periodStart
                        && r.OvertimeDetail.OvertimeDate <= periodEnd)
            .ToListAsync(cancellationToken);

        if (approvedOvertime.Count > 0)
        {
            decimal hourlyRate = salary.BasicSalary / 240m;
            foreach (var ot in approvedOvertime)
            {
                var hours = ot.OvertimeDetail!.ActualHours ?? ot.OvertimeDetail.PlannedHours;
                overtimeAmount += Math.Round((decimal)hours.TotalHours * hourlyRate * ot.OvertimeDetail.Multiplier, 2);
            }
            if (overtimeAmount > 0)
                breakdown.Earnings.Add(new BreakdownItemDto { Name = "Overtime", Amount = overtimeAmount });
        }

        // Loan deductions from active loans
        var activeLoans = await _context.Loans
            .Where(l => !l.IsDeleted && l.IsActive
                        && l.EmployeeId == employeeId.Value
                        && l.StartDate <= periodEnd
                        && l.RemainingAmount > 0)
            .ToListAsync(cancellationToken);

        foreach (var loan in activeLoans)
        {
            var loanDeduction = Math.Min(loan.MonthlyDeduction, loan.RemainingAmount);
            if (loanDeduction > 0)
            {
                deductionTotal += loanDeduction;
                breakdown.Deductions.Add(new BreakdownItemDto { Name = $"Loan: {loan.LoanName}", Amount = loanDeduction });
            }
        }

        breakdown.TotalEarnings = Math.Round(salary.BasicSalary + allowanceTotal + overtimeAmount, 2);
        breakdown.TotalDeductions = Math.Round(deductionTotal, 2);
        breakdown.NetSalary = Math.Round(breakdown.TotalEarnings - breakdown.TotalDeductions, 2);
        breakdown.IsPaid = false;

        return new GenericResponse<SalaryBreakdownDto>
        {
            Success = true,
            Message = "Salary breakdown approximated from current configuration (no payslip found)",
            Data = breakdown
        };
    }
}
