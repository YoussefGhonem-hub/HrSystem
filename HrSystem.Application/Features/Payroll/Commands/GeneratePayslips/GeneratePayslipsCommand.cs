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
        var daysInMonth = DateTime.DaysInMonth(request.Year, request.Month);
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        if (periodStart > currentMonthStart)
            return Error.Validation(description: "Future payroll periods cannot be generated.");

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
            .Where(e => !e.IsDeleted)
            // Only generate payslips for employees whose employment overlaps the period.
            .Where(e => (!e.HiringDate.HasValue || e.HiringDate.Value.Date <= periodEnd)
                && (!e.TerminationDate.HasValue || e.TerminationDate.Value.Date >= periodStart));

        // Apply branch scope for HR managers
        if (branchId.HasValue)
        {
            employeesQuery = employeesQuery.Where(e => e.BranchId == branchId.Value);
        }

        if (request.EmployeeId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.Id == request.EmployeeId.Value);

        var employees = await employeesQuery.ToListAsync(cancellationToken);

        var employeeIds = employees.Select(e => e.Id).ToList();
        var branchIds = employees
            .Where(e => e.BranchId.HasValue && e.BranchId.Value != Guid.Empty)
            .Select(e => e.BranchId!.Value)
            .Distinct()
            .ToList();

        var branchPolicies = await _context.BranchWorkSchedules
            .Where(s => branchIds.Contains(s.BranchId) && s.IsActive && !s.IsDeleted)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        var branchPolicyByBranchId = branchPolicies
            .GroupBy(s => s.BranchId)
            .ToDictionary(g => g.Key, g => g.First());

        // Load public holidays that fall within the period (branch-specific + global).
        var holidaySet = (await _context.BranchHolidays
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.IsActive
                && ((h.Year == request.Year && h.Date.Month == request.Month)
                    || (h.IsRecurring && h.RecurringMonth == request.Month)))
            .Select(h => h.Date.Date)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var attendanceRecords = await _context.Attendances
            .Where(a => !a.IsDeleted
                && !a.IsConfigurationRecord
                && employeeIds.Contains(a.EmployeeId)
                && a.Date >= periodStart
                && a.Date <= periodEnd)
            .ToListAsync(cancellationToken);

        var attendanceByEmployeeId = attendanceRecords
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

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

            var employmentStart = employee.HiringDate.HasValue && employee.HiringDate.Value.Date > periodStart
                ? employee.HiringDate.Value.Date
                : periodStart;
            var employmentEnd = employee.TerminationDate.HasValue && employee.TerminationDate.Value.Date < periodEnd
                ? employee.TerminationDate.Value.Date
                : periodEnd;

            if (employmentEnd < employmentStart)
            {
                result.PayslipsSkipped++;
                result.Warnings.Add($"No payroll period overlap for {employee.FullNameEn} ({employee.EmployeeCode}).");
                continue;
            }

            var payableDays = (employmentEnd - employmentStart).Days + 1;
            // Working days = calendar days minus weekends and public holidays in the employment range.
            branchPolicyByBranchId.TryGetValue(employee.BranchId ?? Guid.Empty, out var empBranchPolicy);
            var expectedWorkingDays = CountWorkingDaysRange(employmentStart, employmentEnd, empBranchPolicy, holidaySet);
            var prorationFactor = Math.Min(1m, Math.Round(payableDays / (decimal)daysInMonth, 6));
            var proratedBasicSalary = Math.Round(currentSalary.BasicSalary * prorationFactor, 2);

            // --- Calculate Allowances ---
            decimal totalAllowances = 0m;
            var payslipAllowances = new List<PayslipAllowance>();

            foreach (var allowance in currentSalary.Allowances.Where(a => !a.IsDeleted))
            {
                var baseAmount = allowance.IsPercentage && allowance.PercentageValue.HasValue
                    ? currentSalary.BasicSalary * (allowance.PercentageValue.Value / 100m)
                    : allowance.Amount;
                var amount = Math.Round(baseAmount * prorationFactor, 2);
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
            decimal grossSalary = proratedBasicSalary + totalAllowances + overtimeAmount;

            // --- Calculate Deductions ---
            decimal totalDeductions = 0m;
            var payslipDeductions = new List<PayslipDeduction>();

            // 1. Configured salary deductions
            foreach (var deduction in currentSalary.Deductions.Where(d => !d.IsDeleted))
            {
                var baseAmount = deduction.IsPercentage && deduction.PercentageValue.HasValue
                    ? currentSalary.BasicSalary * (deduction.PercentageValue.Value / 100m)
                    : deduction.Amount;
                var amount = Math.Round(baseAmount * prorationFactor, 2);
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
                    siEmployee = Math.Round(proratedBasicSalary * (currentSalary.SocialInsuranceEmployeeRate.Value / 100m), 2);
                    totalDeductions += siEmployee;
                }
                if (currentSalary.SocialInsuranceEmployerRate.HasValue && currentSalary.SocialInsuranceEmployerRate.Value > 0)
                {
                    siEmployer = Math.Round(proratedBasicSalary * (currentSalary.SocialInsuranceEmployerRate.Value / 100m), 2);
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

            // 5. Attendance-Based Deductions (minimum work hours policy)
            decimal attendanceDeduction = 0m;
            decimal fullDayAbsentDays = 0m;
            decimal halfDayDays = 0m;

            if (employee.BranchId.HasValue
                && branchPolicyByBranchId.TryGetValue(employee.BranchId.Value, out var branchPolicy)
                && attendanceByEmployeeId.TryGetValue(employee.Id, out var employeeAttendance))
            {
                var attendanceDates = new HashSet<DateTime>();

                foreach (var attendance in employeeAttendance)
                {
                    if (attendance.Date < employmentStart || attendance.Date > employmentEnd)
                        continue;

                    var dateOnly = attendance.Date.Date;
                    // Only count working-day records toward attendance tracking.
                    if (!IsWorkingDay(dateOnly, branchPolicy) || holidaySet.Contains(dateOnly))
                        continue;

                    attendanceDates.Add(dateOnly);

                    if (attendance.StatusId == AttendanceStatusIds.Absent)
                    {
                        fullDayAbsentDays += 1m;
                        continue;
                    }

                    var workedHours = CalculateWorkedHoursForPolicy(attendance, branchPolicy);
                    if (workedHours < branchPolicy.AbsentThresholdHours)
                    {
                        fullDayAbsentDays += 1m;
                    }
                    else if (workedHours >= branchPolicy.MinimumHalfDayHours && workedHours < branchPolicy.MinimumFullDayHours)
                    {
                        halfDayDays += 1m;
                    }
                }

                // Missing = expected working days with no record at all (implicitly absent).
                var missingAttendanceDays = Math.Max(0, expectedWorkingDays - attendanceDates.Count);
                if (missingAttendanceDays > 0)
                {
                    fullDayAbsentDays += missingAttendanceDays;
                }

                if (fullDayAbsentDays > 0 || halfDayDays > 0)
                {
                    var dailySalary = payableDays > 0
                        ? Math.Round(proratedBasicSalary / payableDays, 2)
                        : 0m;
                    attendanceDeduction = Math.Round((fullDayAbsentDays * dailySalary) + (halfDayDays * dailySalary * 0.5m), 2);

                    if (attendanceDeduction > 0)
                    {
                        totalDeductions += attendanceDeduction;
                        payslipDeductions.Add(new PayslipDeduction
                        {
                            DeductionNameAr = "خصم حضور",
                            DeductionNameEn = "Attendance Deduction",
                            Amount = attendanceDeduction
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
                existingPayslip.BasicSalary = proratedBasicSalary;
                existingPayslip.TotalAllowances = totalAllowances;
                existingPayslip.GrossSalary = grossSalary;
                existingPayslip.OvertimeAmount = overtimeAmount;
                existingPayslip.TotalDeductions = totalDeductions;
                existingPayslip.IncomeTax = incomeTax;
                existingPayslip.SocialInsuranceEmployee = siEmployee;
                existingPayslip.SocialInsuranceEmployer = siEmployer;
                existingPayslip.NetSalary = netSalary;
                existingPayslip.TotalWorkingDays = expectedWorkingDays;
                existingPayslip.ActualWorkingDays = Math.Max(0, payableDays - (int)Math.Ceiling(fullDayAbsentDays + halfDayDays));
                existingPayslip.AbsentDays = Math.Max(0, (int)Math.Ceiling(fullDayAbsentDays));
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
                    BasicSalary = proratedBasicSalary,
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
                    TotalWorkingDays = expectedWorkingDays,
                    ActualWorkingDays = Math.Max(0, payableDays - (int)Math.Ceiling(fullDayAbsentDays + halfDayDays)),
                    AbsentDays = Math.Max(0, (int)Math.Ceiling(fullDayAbsentDays)),
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

    private static int CountWorkingDaysRange(
        DateTime from, DateTime to,
        Domain.Entities.Organization.BranchWorkSchedule? schedule,
        HashSet<DateTime> holidays)
    {
        int count = 0;
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            if (IsWorkingDay(day, schedule) && !holidays.Contains(day.Date))
                count++;
        }
        return count;
    }

    private static bool IsWorkingDay(DateTime day, Domain.Entities.Organization.BranchWorkSchedule? schedule)
    {
        if (schedule == null)
            return day.DayOfWeek != DayOfWeek.Friday && day.DayOfWeek != DayOfWeek.Saturday;

        return day.DayOfWeek switch
        {
            DayOfWeek.Sunday => schedule.IsSunday,
            DayOfWeek.Monday => schedule.IsMonday,
            DayOfWeek.Tuesday => schedule.IsTuesday,
            DayOfWeek.Wednesday => schedule.IsWednesday,
            DayOfWeek.Thursday => schedule.IsThursday,
            DayOfWeek.Friday => schedule.IsFriday,
            DayOfWeek.Saturday => schedule.IsSaturday,
            _ => false
        };
    }

    private static decimal CalculateWorkedHoursForPolicy(
        Domain.Entities.Attendance.Attendance attendance,
        Domain.Entities.Organization.BranchWorkSchedule branchPolicy)
    {
        if (!attendance.CheckInTime.HasValue || !attendance.CheckOutTime.HasValue)
            return 0m;

        var worked = attendance.CheckOutTime.Value - attendance.CheckInTime.Value;
        if (branchPolicy.IsBreakTimeDeducted && branchPolicy.BreakDuration.HasValue)
        {
            worked -= branchPolicy.BreakDuration.Value;
            if (worked < TimeSpan.Zero)
                worked = TimeSpan.Zero;
        }

        if (branchPolicy.CheckInWindowMinutes.HasValue && branchPolicy.CheckInWindowMinutes.Value > 0)
        {
            var checkInWindowEnd = branchPolicy.StartTime.Add(TimeSpan.FromMinutes(branchPolicy.CheckInWindowMinutes.Value));
            if (attendance.CheckInTime.Value > checkInWindowEnd)
                return 0m;
        }

        return (decimal)worked.TotalHours;
    }
}
