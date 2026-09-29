using ErrorOr;
using HrSystem.Application.Features.Payroll.Common;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Commands.GeneratePayslips;

/// <summary>
/// Generates payslips for the pay period(s) that belong to the given month/year, according to the
/// organization's payroll cycle settings (calendar month, custom cut-off, bi-weekly or weekly).
/// For bi-weekly/weekly cycles a month may contain several periods; pass <see cref="PeriodStartDate"/>
/// to generate a single one, otherwise every period that has already started is generated.
/// </summary>
public record GeneratePayslipsCommand(
    int Month,
    int Year,
    Guid? EmployeeId = null,
    bool UpdateExisting = false,
    DateTime? PeriodStartDate = null
) : IRequest<ErrorOr<GenericResponse<GeneratePayslipsResultDto>>>;

public class GeneratedPeriodResultDto
{
    public Guid PayrollCycleId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public int PayslipsGenerated { get; set; }
    public int PayslipsSkipped { get; set; }
}

public class GeneratePayslipsResultDto
{
    public Guid PayrollCycleId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public string CycleType { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int PayslipsGenerated { get; set; }
    public int PayslipsSkipped { get; set; }
    public decimal TotalGrossSalary { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalLoanDeductions { get; set; }
    public List<GeneratedPeriodResultDto> Periods { get; set; } = new();
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

        var currentTenantId = CurrentUser.OrganizationId ?? Guid.Empty;
        if (currentTenantId == Guid.Empty)
            return Error.Validation(description: "No organization context is available for payroll generation.");

        // ── Resolve pay periods from the organization's payroll settings ──
        var settings = await PayrollPeriodCalculator.LoadSettingsAsync(_context, currentTenantId, cancellationToken);
        var periods = PayrollPeriodCalculator.GetPeriodsForMonth(settings, request.Month, request.Year).ToList();

        if (request.PeriodStartDate.HasValue)
        {
            var wanted = request.PeriodStartDate.Value.Date;
            periods = periods.Where(p => p.StartDate.Date == wanted).ToList();
            if (periods.Count == 0)
                return Error.Validation(description: $"No pay period for {request.Month}/{request.Year} starts on {wanted:dd/MM/yyyy}.");
        }

        var today = DateTime.UtcNow.Date;
        var futurePeriods = periods.Where(p => p.StartDate.Date > today).ToList();
        var eligiblePeriods = periods.Where(p => p.StartDate.Date <= today).ToList();

        if (eligiblePeriods.Count == 0)
            return Error.Validation(description: "Future payroll periods cannot be generated.");

        var result = new GeneratePayslipsResultDto
        {
            CycleType = (settings?.CycleType ?? PayCycleType.MonthlyCalendar).ToString()
        };

        foreach (var future in futurePeriods)
        {
            result.Warnings.Add($"Skipped pay period {future.RangeLabel}: it has not started yet.");
        }

        // Payslip numbers are PS-YYYYMM-NNNN and must stay unique across all periods of the month.
        var payslipPrefix = $"PS-{request.Year}{request.Month:D2}-";
        var existingPayslipNumbers = await _context.Payslips
            .IgnoreQueryFilters()
            .Where(p => p.PayslipNumber.StartsWith(payslipPrefix))
            .Select(p => p.PayslipNumber)
            .ToListAsync(cancellationToken);

        var payslipCounter = 0;
        foreach (var num in existingPayslipNumbers)
        {
            var parts = num.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts.Last(), out var seq) && seq > payslipCounter)
                payslipCounter = seq;
        }

        var taxBrackets = await _context.TaxBrackets
            .Where(t => t.IsActive && t.Year == request.Year)
            .OrderBy(t => t.MinIncome)
            .ToListAsync(cancellationToken);

        var isSuperOrOrgAdmin = CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true
            || CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true;
        var branchId = isSuperOrOrgAdmin ? (Guid?)null : CurrentUser.BranchId;

        var touchedCycles = new List<PayrollCycle>();

        foreach (var period in eligiblePeriods)
        {
            var cycle = await GetOrCreateCycleAsync(period, currentTenantId, cancellationToken);
            touchedCycles.Add(cycle);

            var periodResult = new GeneratedPeriodResultDto
            {
                PayrollCycleId = cycle.Id,
                CycleName = cycle.CycleName,
                PeriodStartDate = period.StartDate,
                PeriodEndDate = period.EndDate
            };

            payslipCounter = await GenerateForPeriodAsync(
                request, period, cycle, currentTenantId, branchId, taxBrackets,
                payslipPrefix, payslipCounter, result, periodResult, cancellationToken);

            result.Periods.Add(periodResult);
            cycle.StatusId = PayrollStatusIds.Pending;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Recompute cycle totals from the persisted payslips so partial/branch-scoped runs never drift.
        foreach (var cycle in touchedCycles)
        {
            await PayrollCycleTotals.RecalculateAsync(_context, cycle, cancellationToken);
            if (cycle.StatusId == PayrollStatusIds.Draft)
                cycle.StatusId = PayrollStatusIds.Pending;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var firstCycle = touchedCycles[0];
        result.PayrollCycleId = firstCycle.Id;
        result.CycleName = touchedCycles.Count == 1
            ? firstCycle.CycleName
            : string.Join(", ", touchedCycles.Select(c => c.CycleName));

        return GenericResponse<GeneratePayslipsResultDto>.SuccessResult(
            result,
            $"Payslips generated successfully for {result.CycleName}. {result.PayslipsGenerated} created, {result.PayslipsSkipped} skipped.");
    }

    private async Task<PayrollCycle> GetOrCreateCycleAsync(PayPeriod period, Guid tenantId, CancellationToken cancellationToken)
    {
        // Bypass global filters but still scope by tenant so SuperAdmin runs never mix organizations.
        var cycle = await _context.PayrollCycles
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.TenantId == tenantId
                && c.Month == period.Month
                && c.Year == period.Year
                && c.PeriodStartDate == period.StartDate, cancellationToken);

        if (cycle == null)
        {
            cycle = new PayrollCycle
            {
                CycleName = period.CycleName,
                Month = period.Month,
                Year = period.Year,
                PeriodStartDate = period.StartDate,
                PeriodEndDate = period.EndDate,
                StatusId = PayrollStatusIds.Draft,
                TenantId = tenantId
            };
            _context.PayrollCycles.Add(cycle);
            await _context.SaveChangesAsync(cancellationToken);
            return cycle;
        }

        // Keep the stored period in sync with the current settings (name/end date may have been refined).
        cycle.CycleName = period.CycleName;
        cycle.PeriodEndDate = period.EndDate;

        if (cycle.IsDeleted)
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
        }

        await _context.SaveChangesAsync(cancellationToken);
        return cycle;
    }

    /// <summary>
    /// Generates (or updates) the payslips of one pay period. Returns the updated payslip counter.
    /// </summary>
    private async Task<int> GenerateForPeriodAsync(
        GeneratePayslipsCommand request,
        PayPeriod period,
        PayrollCycle cycle,
        Guid tenantId,
        Guid? branchId,
        List<TaxBracket> taxBrackets,
        string payslipPrefix,
        int payslipCounter,
        GeneratePayslipsResultDto result,
        GeneratedPeriodResultDto periodResult,
        CancellationToken cancellationToken)
    {
        var periodStart = period.StartDate.Date;
        var periodEnd = period.EndDate.Date;
        var periodTotalDays = period.TotalDays;

        var employeesQuery = _context.Employees
            .Include(e => e.Salaries).ThenInclude(s => s.Allowances)
            .Include(e => e.Salaries).ThenInclude(s => s.Deductions)
            .Where(e => !e.IsDeleted && e.TenantId == tenantId)
            // Only generate payslips for employees whose employment overlaps the period.
            .Where(e => (!e.HiringDate.HasValue || e.HiringDate.Value.Date <= periodEnd)
                && (!e.TerminationDate.HasValue || e.TerminationDate.Value.Date >= periodStart));

        if (branchId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.BranchId == branchId.Value);

        if (request.EmployeeId.HasValue)
            employeesQuery = employeesQuery.Where(e => e.Id == request.EmployeeId.Value);

        var employees = await employeesQuery.ToListAsync(cancellationToken);
        result.TotalEmployees = Math.Max(result.TotalEmployees, employees.Count);

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

        var holidaysByBranch = await LoadHolidaysByBranchAsync(branchIds, periodStart, periodEnd, cancellationToken);

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

        // Approved overtime requests whose overtime date falls inside the period
        var approvedOvertimeRequests = await _context.EmployeeRequests
            .Include(r => r.OvertimeDetail)
            .Include(r => r.RequestTypeRef)
            .Where(r => r.RequestTypeRef != null && r.RequestTypeRef.Code == "OverTime"
                        && r.Status == EmployeeRequestStatus.Approved
                        && r.OvertimeDetail != null
                        && employeeIds.Contains(r.EmployeeId)
                        && r.OvertimeDetail.OvertimeDate >= periodStart
                        && r.OvertimeDetail.OvertimeDate <= periodEnd)
            .ToListAsync(cancellationToken);

        var overtimeByEmployee = approvedOvertimeRequests
            .GroupBy(r => r.EmployeeId)
            .ToDictionary(g => g.Key, g => g.Select(r => r.OvertimeDetail!).ToList());

        // Active loans that have started by the end of this period (compare on dates to be time-of-day safe).
        var loanCutoff = periodEnd.AddDays(1);
        var activeLoans = await _context.Loans
            .Where(l => !l.IsDeleted && l.IsActive
                        && employeeIds.Contains(l.EmployeeId)
                        && l.StartDate < loanCutoff
                        && l.RemainingAmount > 0
                        // Never deduct beyond the loan's last installment month
                        // (explicit EndDate, else StartDate + (InstallmentMonths - 1)).
                        && (l.EndDate ?? l.StartDate.AddMonths(l.InstallmentMonths > 0 ? l.InstallmentMonths - 1 : 0)) >= periodStart)
            .ToListAsync(cancellationToken);

        var loansByEmployee = activeLoans
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Load existing payslips of this cycle in one query (avoids N+1).
        var existingPayslipsByEmployee = await _context.Payslips
            .Include(p => p.PayslipAllowances)
            .Include(p => p.PayslipDeductions)
            .Where(p => p.PayrollCycleId == cycle.Id && !p.IsDeleted && employeeIds.Contains(p.EmployeeId))
            .ToDictionaryAsync(p => p.EmployeeId, cancellationToken);

        foreach (var employee in employees)
        {
            existingPayslipsByEmployee.TryGetValue(employee.Id, out var existingPayslip);

            if (existingPayslip != null && !request.UpdateExisting)
            {
                result.PayslipsSkipped++;
                periodResult.PayslipsSkipped++;
                result.Warnings.Add($"Payslip already exists for {employee.FullNameEn} ({employee.EmployeeCode}) in {cycle.CycleName}.");
                continue;
            }

            if (existingPayslip?.IsPaid == true)
            {
                result.PayslipsSkipped++;
                periodResult.PayslipsSkipped++;
                result.Warnings.Add($"Payslip already paid for {employee.FullNameEn} ({employee.EmployeeCode}) in {cycle.CycleName}; skipping updates.");
                continue;
            }

            var currentSalary = employee.Salaries
                .Where(s => s.IsCurrent && !s.IsDeleted)
                .OrderByDescending(s => s.EffectiveDate)
                .FirstOrDefault();

            if (currentSalary == null)
            {
                result.PayslipsSkipped++;
                periodResult.PayslipsSkipped++;
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
                periodResult.PayslipsSkipped++;
                result.Warnings.Add($"No payroll period overlap for {employee.FullNameEn} ({employee.EmployeeCode}).");
                continue;
            }

            // ── Period basis ──
            // A monthly salary is converted to the period amount (1 for monthly cycles, 12/26 bi-weekly, 12/52 weekly)
            // and then prorated by the calendar days the employee was employed within the period.
            var payableDays = (employmentEnd - employmentStart).Days + 1;
            var prorationFactor = Math.Min(1m, Math.Round(payableDays / (decimal)periodTotalDays, 6));
            var periodFactor = period.MonthlyAmountFactor * prorationFactor;

            var branchPolicy = ResolveBranchPolicy(employee.BranchId, branchPolicyByBranchId);
            var holidaySet = ResolveHolidaySet(employee.BranchId, holidaysByBranch);
            var expectedWorkingDays = CountWorkingDaysRange(employmentStart, employmentEnd, branchPolicy, holidaySet);

            var proratedBasicSalary = Math.Round(currentSalary.BasicSalary * periodFactor, 2);

            // --- Allowances ---
            decimal totalAllowances = 0m;
            var payslipAllowances = new List<PayslipAllowance>();

            foreach (var allowance in currentSalary.Allowances.Where(a => !a.IsDeleted))
            {
                var baseAmount = allowance.IsPercentage && allowance.PercentageValue.HasValue
                    ? currentSalary.BasicSalary * (allowance.PercentageValue.Value / 100m)
                    : allowance.Amount;
                var amount = Math.Round(baseAmount * periodFactor, 2);
                totalAllowances += amount;
                payslipAllowances.Add(new PayslipAllowance
                {
                    AllowanceNameAr = allowance.NameAr,
                    AllowanceNameEn = allowance.NameEn,
                    Amount = amount
                });
            }

            // --- Overtime from approved requests (hourly rate: monthly basic / 240h) ---
            decimal overtimeAmount = 0m;
            if (overtimeByEmployee.TryGetValue(employee.Id, out var overtimeDetails))
            {
                decimal hourlyRate = currentSalary.BasicSalary / 240m;
                foreach (var ot in overtimeDetails)
                {
                    var hours = ot.ActualHours ?? ot.PlannedHours;
                    overtimeAmount += Math.Round((decimal)hours.TotalHours * hourlyRate * ot.Multiplier, 2);
                }
            }

            decimal grossSalary = proratedBasicSalary + totalAllowances + overtimeAmount;

            // --- Deductions ---
            decimal totalDeductions = 0m;
            var payslipDeductions = new List<PayslipDeduction>();

            // 1. Configured salary deductions
            foreach (var deduction in currentSalary.Deductions.Where(d => !d.IsDeleted))
            {
                var baseAmount = deduction.IsPercentage && deduction.PercentageValue.HasValue
                    ? currentSalary.BasicSalary * (deduction.PercentageValue.Value / 100m)
                    : deduction.Amount;
                var amount = Math.Round(baseAmount * periodFactor, 2);
                totalDeductions += amount;
                payslipDeductions.Add(new PayslipDeduction
                {
                    DeductionNameAr = deduction.NameAr,
                    DeductionNameEn = deduction.NameEn,
                    Amount = amount
                });
            }

            // 2. Attendance-based absence (absent / half days against the branch work schedule).
            //    Computed first because insurance and tax are based on what the employee actually earned.
            decimal attendanceDeduction = 0m;
            decimal fullDayAbsentDays = 0m;
            decimal halfDayDays = 0m;

            attendanceByEmployeeId.TryGetValue(employee.Id, out var employeeAttendance);
            employeeAttendance ??= new List<Domain.Entities.Attendance.Attendance>();

            var attendanceDates = new HashSet<DateTime>();
            foreach (var attendance in employeeAttendance)
            {
                var dateOnly = attendance.Date.Date;
                if (dateOnly < employmentStart || dateOnly > employmentEnd)
                    continue;

                // Only working-day records count toward attendance tracking.
                if (!IsWorkingDay(dateOnly, branchPolicy) || holidaySet.Contains(dateOnly))
                    continue;

                // One row per day; a second row for the same day must not double count.
                if (!attendanceDates.Add(dateOnly))
                    continue;

                if (attendance.StatusId == AttendanceStatusIds.Absent)
                {
                    fullDayAbsentDays += 1m;
                    continue;
                }

                if (attendance.StatusId == AttendanceStatusIds.OnLeave
                    || attendance.StatusId == AttendanceStatusIds.Holiday
                    || attendance.StatusId == AttendanceStatusIds.Weekend)
                {
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

            // Expected working days with no record at all are implicitly absent.
            var missingAttendanceDays = Math.Max(0, expectedWorkingDays - attendanceDates.Count);
            fullDayAbsentDays += missingAttendanceDays;

            if (fullDayAbsentDays > 0 || halfDayDays > 0)
            {
                // Daily rate is based on the expected working days of the period, so a full-period absence
                // deducts the whole (prorated) basic salary.
                var dailySalary = expectedWorkingDays > 0
                    ? proratedBasicSalary / expectedWorkingDays
                    : 0m;
                attendanceDeduction = Math.Round((fullDayAbsentDays * dailySalary) + (halfDayDays * dailySalary * 0.5m), 2);
                attendanceDeduction = Math.Min(attendanceDeduction, proratedBasicSalary);
            }

            // Earnings after absence: the base for social insurance (basic) and income tax (gross).
            var basicAfterAbsence = Math.Max(0m, proratedBasicSalary - attendanceDeduction);
            var taxableGross = Math.Max(0m, grossSalary - attendanceDeduction);

            // 3. Social insurance
            decimal siEmployee = 0m;
            decimal siEmployer = 0m;
            if (currentSalary.IsSocialInsuranceEnabled)
            {
                if (currentSalary.SocialInsuranceEmployeeRate is > 0)
                {
                    siEmployee = Math.Round(basicAfterAbsence * (currentSalary.SocialInsuranceEmployeeRate.Value / 100m), 2);
                    totalDeductions += siEmployee;
                }
                if (currentSalary.SocialInsuranceEmployerRate is > 0)
                {
                    siEmployer = Math.Round(basicAfterAbsence * (currentSalary.SocialInsuranceEmployerRate.Value / 100m), 2);
                }
            }

            // 4. Income tax (progressive brackets on annualized taxable gross)
            decimal incomeTax = 0m;
            if (taxBrackets.Count > 0 && taxableGross > 0)
            {
                decimal annualGross = taxableGross * period.PeriodsPerYear;
                decimal annualTax = CalculateProgressiveTax(annualGross, taxBrackets);
                incomeTax = Math.Round(annualTax / period.PeriodsPerYear, 2);
                totalDeductions += incomeTax;
            }

            // 5. Loan installments (balances are reduced only when the payslip is marked as paid)
            decimal totalLoanDeduction = 0m;
            if (loansByEmployee.TryGetValue(employee.Id, out var employeeLoans))
            {
                foreach (var loan in employeeLoans)
                {
                    var installment = Math.Round(loan.MonthlyDeduction * period.MonthlyAmountFactor, 2);
                    var deductionAmount = Math.Min(installment, loan.RemainingAmount);
                    if (deductionAmount <= 0)
                        continue;

                    totalLoanDeduction += deductionAmount;
                    totalDeductions += deductionAmount;
                    payslipDeductions.Add(new PayslipDeduction
                    {
                        DeductionNameAr = $"قسط قرض: {loan.LoanName}",
                        DeductionNameEn = $"Loan: {loan.LoanName}",
                        Amount = deductionAmount,
                        LoanId = loan.Id
                    });
                }
            }

            // 6. Attendance deduction row (amount computed in step 2)
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

            decimal netSalary = grossSalary - totalDeductions;

            var absentDaysInt = (int)Math.Ceiling(fullDayAbsentDays);
            var actualWorkingDays = Math.Max(0, expectedWorkingDays - (int)Math.Ceiling(fullDayAbsentDays + halfDayDays));

            if (existingPayslip != null)
            {
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
                existingPayslip.ActualWorkingDays = actualWorkingDays;
                existingPayslip.AbsentDays = absentDaysInt;
                existingPayslip.GeneratedDate = DateTime.UtcNow;

                _context.PayslipAllowances.RemoveRange(existingPayslip.PayslipAllowances.ToList());
                _context.PayslipDeductions.RemoveRange(existingPayslip.PayslipDeductions.ToList());
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
            }
            else
            {
                payslipCounter++;
                var payslip = new Payslip
                {
                    PayrollCycleId = cycle.Id,
                    EmployeeId = employee.Id,
                    PayslipNumber = $"{payslipPrefix}{payslipCounter:D4}",
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
                    ActualWorkingDays = actualWorkingDays,
                    AbsentDays = absentDaysInt,
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
            }

            result.PayslipsGenerated++;
            periodResult.PayslipsGenerated++;
            result.TotalGrossSalary += grossSalary;
            result.TotalNetSalary += netSalary;
            result.TotalDeductions += totalDeductions;
            result.TotalOvertimeAmount += overtimeAmount;
            result.TotalLoanDeductions += totalLoanDeduction;
        }

        return payslipCounter;
    }

    private async Task<Dictionary<Guid, HashSet<DateTime>>> LoadHolidaysByBranchAsync(
        List<Guid> branchIds,
        DateTime periodStart,
        DateTime periodEnd,
        CancellationToken cancellationToken)
    {
        var rows = await _context.BranchHolidays
            .AsNoTracking()
            .Where(h => !h.IsDeleted && h.IsActive && branchIds.Contains(h.BranchId)
                && (h.IsRecurring || (h.Date >= periodStart && h.Date < periodEnd.AddDays(1))))
            .Select(h => new { h.BranchId, h.Date, h.IsRecurring, h.RecurringMonth, h.RecurringDay })
            .ToListAsync(cancellationToken);

        var result = new Dictionary<Guid, HashSet<DateTime>>();
        foreach (var row in rows)
        {
            if (!result.TryGetValue(row.BranchId, out var set))
            {
                set = new HashSet<DateTime>();
                result[row.BranchId] = set;
            }

            if (!row.IsRecurring)
            {
                set.Add(row.Date.Date);
                continue;
            }

            var month = row.RecurringMonth ?? row.Date.Month;
            var day = row.RecurringDay ?? row.Date.Day;
            for (var year = periodStart.Year; year <= periodEnd.Year; year++)
            {
                if (month < 1 || month > 12 || day < 1 || day > DateTime.DaysInMonth(year, month))
                    continue;

                var date = new DateTime(year, month, day);
                if (date >= periodStart && date <= periodEnd)
                    set.Add(date);
            }
        }

        return result;
    }

    private static HashSet<DateTime> ResolveHolidaySet(Guid? branchId, Dictionary<Guid, HashSet<DateTime>> holidaysByBranch)
    {
        return branchId.HasValue && holidaysByBranch.TryGetValue(branchId.Value, out var set)
            ? set
            : new HashSet<DateTime>();
    }

    /// <summary>
    /// Returns the branch's active work schedule, or the default schedule (Sun–Thu, 09:00–17:00) when
    /// the branch has none, so attendance-based deductions are always applied consistently.
    /// </summary>
    private static BranchWorkSchedule ResolveBranchPolicy(Guid? branchId, Dictionary<Guid, BranchWorkSchedule> policies)
    {
        if (branchId.HasValue && policies.TryGetValue(branchId.Value, out var policy))
            return policy;

        return new BranchWorkSchedule();
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
        BranchWorkSchedule schedule,
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

    private static bool IsWorkingDay(DateTime day, BranchWorkSchedule schedule)
    {
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
        BranchWorkSchedule branchPolicy)
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

        if (branchPolicy.CheckInWindowMinutes is > 0)
        {
            var checkInWindowEnd = branchPolicy.StartTime.Add(TimeSpan.FromMinutes(branchPolicy.CheckInWindowMinutes.Value));
            if (attendance.CheckInTime.Value > checkInWindowEnd)
                return 0m;
        }

        return (decimal)worked.TotalHours;
    }
}
