using HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Leave.Queries.GetMyLeaveBalances;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;

public class EmployeeDetailsDto
{
    public EmployeeDto Employee { get; set; } = null!;
    public EmployeePayrollSummaryDto Payroll { get; set; } = new();
    public EmployeeAttendanceSectionDto Attendance { get; set; } = new();
    public List<LeaveBalanceDto> LeaveBalances { get; set; } = new();
    public List<EmployeeDocumentDto> Documents { get; set; } = new();
    public List<EmployeeAssetDto> Assets { get; set; } = new();
}

public class EmployeeAttendanceSectionDto
{
    public int PresentDays { get; set; }
    public int LateDays { get; set; }
    public int AbsentDays { get; set; }
    public double OvertimeHours { get; set; }
    public List<EmployeeAttendanceHistoryItemDto> History { get; set; } = new();
}

public class EmployeeAttendanceHistoryItemDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public Guid StatusId { get; set; }
    public string StatusNameEn { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public TimeSpan? WorkedHours { get; set; }
}

public class EmployeePayrollSummaryDto
{
    public Guid? PayslipId { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime? GeneratedDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PayDayDescription { get; set; }
    public string Currency { get; set; } = string.Empty;
    public List<EmployeePayslipHistoryItemDto> History { get; set; } = new();
}

public class EmployeePayslipHistoryItemDto
{
    public Guid PayslipId { get; set; }
    public string CycleName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty; // e.g., "Paid", "Processing"
}

public class EmployeeDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? DocumentTypeNameEn { get; set; }
    public string? DocumentTypeNameAr { get; set; }
    public string? FileUrl { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class EmployeeAssetDto
{
    public Guid Id { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsReturned { get; set; }
}
