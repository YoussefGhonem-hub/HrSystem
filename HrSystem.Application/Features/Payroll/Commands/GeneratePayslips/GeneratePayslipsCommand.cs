using ErrorOr;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.GeneratePayslips;

public record GeneratePayslipsCommand(
    int Month,
    int Year,
    Guid? EmployeeId = null,
    bool UpdateExisting = false
) : IRequest<ErrorOr<GenericResponse<GeneratePayslipsResultDto>>>;

public class GeneratePayslipsResultDto
{
    public Guid PayrollCycleId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int PayslipsGenerated { get; set; }
    public int PayslipsSkipped { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalLoanDeductions { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class GeneratePayslipsCommandHandler
    : IRequestHandler<GeneratePayslipsCommand, ErrorOr<GenericResponse<GeneratePayslipsResultDto>>>
{
    private readonly ApplicationDbContext _context;

    public GeneratePayslipsCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<GeneratePayslipsResultDto>>> Handle(
        GeneratePayslipsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Month < 1 || request.Month > 12)
            return Error.Validation(description: "Month must be between 1 and 12.");
        if (request.Year < 2000 || request.Year > 2100)
            return Error.Validation(description: "Invalid year.");

        var periodStart = new DateTime(request.Year, request.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var currentTenantId = CurrentUser.OrganizationId ?? Guid.Empty;

        // Get or create payroll cycle (bypass global filters but still scope by tenant)
        var cycle = await _context.PayrollCycles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Month == request.Month && c.Year == request.Year
                && c.TenantId == currentTenantId, cancellationToken);

        if (cycle == null)
        {
            cycle = new PayrollCycle
            {
                CycleName = periodStart.ToString("MMMM yyyy"),
                Month = request.Month,
                Year = request.Year,
                PeriodStartDate = periodStart,
                PeriodEndDate = periodEnd,
                StatusId = PayrollStatusIds.Draft
            };
            _context.PayrollCycles.Add(cycle);
            await _context.SaveChangesAsync(cancellationToken);
        }
        else if (cycle.IsDeleted)
        {
            // Reactivate a previously soft-deleted cycle
            cycle.IsDeleted = false;
            cycle.DeletedDate = null;
            cycle.DeletedBy = null;
            cycle.StatusId = PayrollStatusIds.Draft;
            cycle.TotalGrossSalary = 0;
            cycle.TotalNetSalary = 0;
            cycle.TotalDeductions = 0;
            cycle.TotalTax = 0;
            cycle.TotalInsurance = 0;
            await _context.SaveChangesAsync(cancellationToken);
        }

        // Get employees with current salary configuration
        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var employeesQuery = _context.Employees
            .Include(e => e.Salaries).ThenInclude(s => s.Allowances)
            .Include(e => e.Salaries).ThenInclude(s => s.Deductions)
            .Where(e => !e.IsDeleted);

        // Apply branch scope for HR managers
        if (branchId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.BranchId == branchId.Value);
        }

        if (request.EmployeeId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.Id == request.EmployeeId.Value);

        var employees = await employeesQuery.ToListAsync(cancellationToken);

        // Get the max existing payslip counter for this month/year (including soft-deleted records
        // and records from other cycles) to avoid PayslipNumber unique constraint violations
        var payslipPrefix = $"PS-{request.Year}{request.Month:D2}-";
        var existingPayslipNumbers = await _context.Payslips
            .IgnoreQueryFilters()
            .Where(p => p.PayslipNumber.StartsWith(payslipPrefix))
            .Select(p => p.PayslipNumber)
            .ToListAsync(cancellationToken);

        int payslipCounter = 0;
        foreach (var num in existingPayslipNumbers)
        {
            var parts = num.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts.Last(), out var seq) && seq > payslipCounter)
                payslipCounter = seq;
        }

        // Get approved overtime requests for the period
        var approvedOvertimeRequests = await _context.EmployeeRequests
            .Include(r => r.OvertimeDetail)
            .Include(r => r.RequestTypeRef)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime"
                        && r.Status == EmployeeRequestStatus.Approved
                        && r.OvertimeDetail != null
                        && r.OvertimeDetail.OvertimeDate >= periodStart
                        && r.OvertimeDetail.OvertimeDate <= periodEnd)
            .ToListAsync(cancellationToken);

        // Group overtime by employee
        var overtimeByEmployee = approvedOvertimeRequests
            .GroupBy(r => r.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => r.OvertimeDetail!).ToList());

        // Get active loans whose StartDate falls within or before this payroll period.
        // Use < periodStart.AddMonths(1) (i.e. < first day of next month) to be time-of-day safe
        // and to ensure a loan approved on the last day of this month is still included.
        var nextPeriodStart = periodStart.AddMonths(1);
        var activeLoans = await _context.Loans
            .Where(l => !l.IsDeleted && l.IsActive
                        && l.StartDate < nextPeriodStart
                        && l.RemainingAmount > 0)
            .ToListAsync(cancellationToken);

        var loansByEmployee = activeLoans
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Get tax brackets for the year
        var taxBrackets = await _context.TaxBrackets
            .Where(t => t.IsActive && t.Year == request.Year)
            .OrderBy(t => t.MinIncome)
            .ToListAsync(cancellationToken);

        var result = new GeneratePayslipsResultDto
        {
            PayrollCycleId = cycle.Id,
            CycleName = cycle.CycleName,
            TotalEmployees = employees.Count
        };

        foreach (var employee in employees)
        {
            // Check if payslip exists
            var existingPayslip = await _context.Payslips
                .Include(p => p.PayslipAllowances)
                .Include(p => p.PayslipDeductions)
                .FirstOrDefaultAsync(p => p.PayrollCycleId == cycle.Id 
                    && p.EmployeeId == employee.Id 
                    && !p.IsDeleted, cancellationToken);

            if (existingPayslip != null && !request.UpdateExisting)
            {
                result.PayslipsSkipped++;
                result.Warnings.Add($"Payslip already exists for {employee.FullNameEn} ({employee.EmployeeCode}).");
                continue;
            }

            if (existingPayslip?.IsPaid == true)
            {
                result.PayslipsSkipped++;
                result.Warnings.Add($"Payslip already paid for {employee.FullNameEn} ({employee.EmployeeCode}); skipping updates.");
                continue;
            }

            var currentSalary = employee.Salaries
                .Where(s => s.IsCurrent && !s.IsDeleted)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefault();

            if (currentSalary == null)
            {
                result.PayslipsSkipped++;
                result.Warnings.Add($"No salary configuration for {employee.FullNameEn} ({employee.EmployeeCode}).");
                continue;
            }

            // --- Calculate Allowances ---
            decimal totalAllowances = 0m;
            var payslipAllowances = new List<PayslipAllowance>();

            foreach (var allowance in currentSalary.Allowances.Where(a => !a.IsDeleted))
            {
                var amount = allowance.IsPercentage && allowance.PercentageValue.HasValue
                    ? Math.Round(currentSalary.BasicSalary * (allowance.PercentageValue.Value / 100m), 2)
                    : allowance.Amount;
                totalAllowances += amount;
                payslipAllowances.Add(new PayslipAllowance
                {
                    AllowanceNameAr = allowance.NameAr,
                    AllowanceNameEn = allowance.NameEn,
                    Amount = amount
                });
            }

            // --- Calculate Overtime from Approved Requests ---
            decimal overtimeAmount = 0m;
            if (overtimeByEmployee.TryGetValue(employee.Id, out var overtimeDetails))
            {
                // Calculate hourly rate from basic salary (assuming 30 days, 8 hours/day)
                decimal hourlyRate = currentSalary.BasicSalary / 240m;

                foreach (var ot in overtimeDetails)
                {
                    var hours = ot.ActualHours ?? ot.PlannedHours;
                    var otPay = Math.Round((decimal)hours.TotalHours * hourlyRate * ot.Multiplier, 2);
                    overtimeAmount += otPay;
                }
            }

            // --- Gross Salary ---
            decimal grossSalary = currentSalary.BasicSalary + totalAllowances + overtimeAmount;

            // --- Calculate Deductions ---
            decimal totalDeductions = 0m;
            var payslipDeductions = new List<PayslipDeduction>();

            // 1. Configured salary deductions
            foreach (var deduction in currentSalary.Deductions.Where(d => !d.IsDeleted))
            {
                var amount = deduction.IsPercentage && deduction.PercentageValue.HasValue
                    ? Math.Round(currentSalary.BasicSalary * (deduction.PercentageValue.Value / 100m), 2)
                    : deduction.Amount;
                totalDeductions += amount;
                payslipDeductions.Add(new PayslipDeduction
                {
                    DeductionNameAr = deduction.NameAr,
                    DeductionNameEn = deduction.NameEn,
                    Amount = amount
                });
            }

            // 2. Social Insurance
            decimal siEmployee = 0m;
            decimal siEmployer = 0m;
            if (currentSalary.IsSocialInsuranceEnabled)
            {
                if (currentSalary.SocialInsuranceEmployeeRate.HasValue && currentSalary.SocialInsuranceEmployeeRate.Value > 0)
                {
                    siEmployee = Math.Round(currentSalary.BasicSalary * (currentSalary.SocialInsuranceEmployeeRate.Value / 100m), 2);
                    totalDeductions += siEmployee;
                }
                if (currentSalary.SocialInsuranceEmployerRate.HasValue && currentSalary.SocialInsuranceEmployerRate.Value > 0)
                {
                    siEmployer = Math.Round(currentSalary.BasicSalary * (currentSalary.SocialInsuranceEmployerRate.Value / 100m), 2);
                }
            }

            // 3. Income Tax (progressive brackets)
            decimal incomeTax = 0m;
            if (taxBrackets.Count > 0)
            {
                // Annualize gross for bracket calculation, then get monthly
                decimal annualGross = grossSalary * 12m;
                decimal annualTax = CalculateProgressiveTax(annualGross, taxBrackets);
                incomeTax = Math.Round(annualTax / 12m, 2);
                totalDeductions += incomeTax;
            }

            // 4. Loan Deductions
            // Deduction rows are attached during payslip generation, but loan balances are
            // only reduced when HR marks the payslip as paid.
            decimal totalLoanDeduction = 0m;
            if (loansByEmployee.TryGetValue(employee.Id, out var employeeLoans))
            {
                foreach (var loan in employeeLoans)
                {
                    var deductionAmount = Math.Min(loan.MonthlyDeduction, loan.RemainingAmount);
                    if (deductionAmount > 0)
                    {
                        totalLoanDeduction += deductionAmount;
                        totalDeductions += deductionAmount;
                        payslipDeductions.Add(new PayslipDeduction
                        {
                            DeductionNameAr = $"قسط قرض: {loan.LoanName}",
                            DeductionNameEn = $"Loan: {loan.LoanName}",
                            Amount = deductionAmount,
                            LoanId = loan.Id   // link for future restoration on regeneration
                        });
                    }
                }
            }

            // --- Net Salary ---
            decimal netSalary = grossSalary - totalDeductions;

            // --- Create or Update Payslip ---
            if (existingPayslip != null)
            {
                // Update existing payslip
                existingPayslip.BasicSalary = currentSalary.BasicSalary;
                existingPayslip.TotalAllowances = totalAllowances;
                existingPayslip.GrossSalary = grossSalary;
                existingPayslip.OvertimeAmount = overtimeAmount;
                existingPayslip.TotalDeductions = totalDeductions;
                existingPayslip.IncomeTax = incomeTax;
                existingPayslip.SocialInsuranceEmployee = siEmployee;
                existingPayslip.SocialInsuranceEmployer = siEmployer;
                existingPayslip.NetSalary = netSalary;
                existingPayslip.GeneratedDate = DateTime.UtcNow;

                // Remove old allowances/deductions and add new ones
                var oldAllowances = existingPayslip.PayslipAllowances.ToList();
                var oldDeductions = existingPayslip.PayslipDeductions.ToList();
                _context.PayslipAllowances.RemoveRange(oldAllowances);
                _context.PayslipDeductions.RemoveRange(oldDeductions);
                existingPayslip.PayslipAllowances.Clear();
                existingPayslip.PayslipDeductions.Clear();

                foreach (var pa in payslipAllowances)
                {
                    pa.PayslipId = existingPayslip.Id;
                    _context.PayslipAllowances.Add(pa);
                }

                foreach (var pd in payslipDeductions)
                {
                    pd.PayslipId = existingPayslip.Id;
                    _context.PayslipDeductions.Add(pd);
                }

                result.PayslipsGenerated++;
            }
            else
            {
                // Create new payslip
                payslipCounter++;
                var payslipNumber = $"PS-{request.Year}{request.Month:D2}-{payslipCounter:D4}";

                var payslip = new Payslip
                {
                    PayrollCycleId = cycle.Id,
                    EmployeeId = employee.Id,
                    PayslipNumber = payslipNumber,
                    BasicSalary = currentSalary.BasicSalary,
                    TotalAllowances = totalAllowances,
                    GrossSalary = grossSalary,
                    OvertimeAmount = overtimeAmount,
                    BonusAmount = 0,
                    TotalDeductions = totalDeductions,
                    IncomeTax = incomeTax,
                    SocialInsuranceEmployee = siEmployee,
                    SocialInsuranceEmployer = siEmployer,
                    LeaveDeductions = 0,
                    UnpaidLeaveDays = 0,
                    NetSalary = netSalary,
                    TotalWorkingDays = DateTime.DaysInMonth(request.Year, request.Month),
                    ActualWorkingDays = DateTime.DaysInMonth(request.Year, request.Month),
                    AbsentDays = 0,
                    GeneratedDate = DateTime.UtcNow,
                    TenantId = employee.TenantId,
                    BranchId = employee.BranchId
                };

                foreach (var pa in payslipAllowances)
                {
                    pa.PayslipId = payslip.Id;
                    payslip.PayslipAllowances.Add(pa);
                }

                foreach (var pd in payslipDeductions)
                {
                    pd.PayslipId = payslip.Id;
                    payslip.PayslipDeductions.Add(pd);
                }

                _context.Payslips.Add(payslip);

                result.PayslipsGenerated++;
            }
            result.TotalGrossSalary += grossSalary;
            result.TotalNetSalary += netSalary;
            result.TotalDeductions += totalDeductions;
            result.TotalOvertimeAmount += overtimeAmount;
            result.TotalLoanDeductions += totalLoanDeduction;
        }

        // Update cycle totals (reset when updating to avoid accumulation)
        if (request.UpdateExisting)
        {
            cycle.TotalGrossSalary = result.TotalGrossSalary;
            cycle.TotalNetSalary = result.TotalNetSalary;
            cycle.TotalDeductions = result.TotalDeductions;
            cycle.TotalTax = result.TotalDeductions;
        }
        else
        {
            cycle.TotalGrossSalary += result.TotalGrossSalary;
            cycle.TotalNetSalary += result.TotalNetSalary;
            cycle.TotalDeductions += result.TotalDeductions;
            cycle.TotalTax += result.TotalDeductions;
        }
        cycle.StatusId = PayrollStatusIds.Pending;

        await _context.SaveChangesAsync(cancellationToken);

        return GenericResponse<GeneratePayslipsResultDto>.SuccessResult(
            result,
            $"Payslips generated successfully. {result.PayslipsGenerated} created, {result.PayslipsSkipped} skipped.");
    }

    private static decimal CalculateProgressiveTax(decimal annualIncome, List<TaxBracket> brackets)
    {
        decimal totalTax = 0m;

        foreach (var bracket in brackets)
        {
            if (annualIncome <= bracket.MinIncome)
                break;

            var taxableInBracket = Math.Min(annualIncome, bracket.MaxIncome) - bracket.MinIncome;
            if (taxableInBracket > 0)
            {
                totalTax += bracket.FixedAmount + (taxableInBracket * (bracket.TaxRate / 100m));
            }
        }

        return totalTax;
    }
}
