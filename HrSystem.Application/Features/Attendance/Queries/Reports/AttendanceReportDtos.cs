namespace HrSystem.Application.Features.Attendance.Queries.Reports;

// ─────────────────────────────────────────────
//  Shared metadata block for every report type
// ─────────────────────────────────────────────
public class AttendanceReportMeta
{
    public string ReportType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string? FilteredByDepartment { get; set; }
    public string? FilteredByEmployee { get; set; }
}

// ─────────────────────────────────────────────
//  1. Daily Attendance Report
// ─────────────────────────────────────────────
public class DailyAttendanceReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public DateTime ReportDate { get; set; }
    public int TotalEmployees { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int EarlyLeaveCount { get; set; }
    public int OnLeaveCount { get; set; }
    public int WorkFromHomeCount { get; set; }
    public double AttendanceRate { get; set; }
    public List<DailyAttendanceRowDto> Rows { get; set; } = new();
}

public class DailyAttendanceRowDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public TimeSpan? CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public int LateMinutes { get; set; }
    public int EarlyLeaveMinutes { get; set; }
    public double WorkedHours { get; set; }
}

// ─────────────────────────────────────────────
//  2. Monthly Attendance Summary Report
// ─────────────────────────────────────────────
public class MonthlyAttendanceSummaryReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public List<EmployeeMonthlyAttendanceDto> Rows { get; set; } = new();
}

public class EmployeeMonthlyAttendanceDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public int DaysPresent { get; set; }
    public int DaysAbsent { get; set; }
    public int DaysLate { get; set; }
    public double TotalLateMinutes { get; set; }
    public double TotalLateHours => Math.Round(TotalLateMinutes / 60.0, 2);
    public double OvertimeHours { get; set; }
    public int LeaveDaysTaken { get; set; }
    public double AttendancePercentage { get; set; }
}

// ─────────────────────────────────────────────
//  3. Payroll Attendance Report
// ─────────────────────────────────────────────
public class PayrollAttendanceReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public int Month { get; set; }
    public int Year { get; set; }
    public int TotalWorkingDays { get; set; }
    public decimal TotalLateDeductions { get; set; }
    public decimal TotalOvertimeAmounts { get; set; }
    public List<PayrollAttendanceRowDto> Rows { get; set; } = new();
}

public class PayrollAttendanceRowDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public decimal BasicSalary { get; set; }
    public int TotalWorkingDays { get; set; }
    public int WorkedDays { get; set; }
    public int AbsentDays { get; set; }
    public double LateMinutes { get; set; }
    public decimal DailyRate { get; set; }
    public decimal AbsentDeductionAmount { get; set; }
    public decimal LateDeductionAmount { get; set; }
    public double OvertimeHours { get; set; }
    public decimal OvertimeRate { get; set; }
    public decimal OvertimeAmount { get; set; }
    /// <summary>
    /// Positive = net gain (OT > deductions); Negative = net loss
    /// </summary>
    public decimal NetSalaryImpact { get; set; }
}

// ─────────────────────────────────────────────
//  4. Late Arrivals Report
// ─────────────────────────────────────────────
public class LateArrivalsReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public int TotalLateInstances { get; set; }
    public List<LateArrivalRowDto> Rows { get; set; } = new();
    public List<LateArrivalEmployeeSummaryDto> EmployeeSummary { get; set; } = new();
}

public class LateArrivalRowDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan? ScheduledStartTime { get; set; }
    public TimeSpan? ActualCheckIn { get; set; }
    public int MinutesLate { get; set; }
}

public class LateArrivalEmployeeSummaryDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int LateFrequency { get; set; }
    public double TotalLateMinutes { get; set; }
    public double AverageLateMinutes { get; set; }
}

// ─────────────────────────────────────────────
//  5. Absenteeism Report
// ─────────────────────────────────────────────
public class AbsenteeismReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public int TotalAbsentDays { get; set; }
    public double OverallAbsenceRate { get; set; }
    public List<AbsenteeismRowDto> Rows { get; set; } = new();
    public List<DepartmentAbsenceSummaryDto> DepartmentSummary { get; set; } = new();
}

public class AbsenteeismRowDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int AbsentDays { get; set; }
    public int AuthorizedAbsenceDays { get; set; }
    public int UnauthorizedAbsenceDays { get; set; }
    public double AbsenceRate { get; set; }
    public List<string> AbsenceReasons { get; set; } = new();
}

public class DepartmentAbsenceSummaryDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public int TotalAbsentDays { get; set; }
    public double AbsenceRate { get; set; }
}

// ─────────────────────────────────────────────
//  6. Overtime Report
// ─────────────────────────────────────────────
public class OvertimeReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public double TotalOvertimeHours { get; set; }
    public decimal TotalOvertimeCost { get; set; }
    public List<OvertimeRowDto> Rows { get; set; } = new();
    public List<DepartmentOvertimeSummaryDto> DepartmentSummary { get; set; } = new();
}

public class OvertimeRowDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public double RegularHours { get; set; }
    public double OvertimeHours { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal OvertimeCost { get; set; }
}

public class DepartmentOvertimeSummaryDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public double TotalOvertimeHours { get; set; }
    public decimal TotalOvertimeCost { get; set; }
}

// ─────────────────────────────────────────────
//  7. Department Attendance Report
// ─────────────────────────────────────────────
public class DepartmentAttendanceReportDto
{
    public AttendanceReportMeta Meta { get; set; } = new();
    public int TotalWorkingDays { get; set; }
    public List<DepartmentAttendanceRowDto> Rows { get; set; } = new();
}

public class DepartmentAttendanceRowDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalEmployees { get; set; }
    public double AverageAttendancePercentage { get; set; }
    public int TotalAbsentDays { get; set; }
    public int TotalLateInstances { get; set; }
    public double TotalOvertimeHours { get; set; }
}
