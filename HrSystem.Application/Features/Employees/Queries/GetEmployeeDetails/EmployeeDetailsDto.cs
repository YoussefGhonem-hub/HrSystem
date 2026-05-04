using System;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;

public class EmployeeDetailsDto
{
    public EmployeePersonalInfoDetailsDto PersonalInfo { get; set; } = new();
    public EmployeeJobInfoDetailsDto? JobInfo { get; set; }
    public EmployeePayrollDetailsDto? Payroll { get; set; }
    public EmployeeAttendanceDetailsDto? Attendance { get; set; }
    public List<EmployeeDocumentGroupDetailsDto> Documents { get; set; } = new();
    public List<EmployeeAssetDetailsDto> Assets { get; set; } = new();
    public EmployeeBalanceSnapshotDto Balances { get; set; } = new();
}

public class EmployeePersonalInfoDetailsDto
{
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public List<EmployeeRoleDto> Roles { get; set; } = new();
    public string NationalId { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Guid GenderId { get; set; }
    public Guid MaritalStatusId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string AddressAr { get; set; } = string.Empty;
    public string? AddressEn { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class EmployeeJobInfoDetailsDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string EmploymentStatusNameEn { get; set; } = string.Empty;
    public string EmploymentStatusNameAr { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? DirectManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ContractTypeId { get; set; }
    public DateTime? HiringDate { get; set; }
    public int? ProbationPeriodMonths { get; set; }
    public Guid? RoleId { get; set; }
    public string? RoleNameEn { get; set; }
    public string? RoleNameAr { get; set; }
}

public class EmployeePayrollDetailsDto
{
    public decimal BasicSalary { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Currency { get; set; }
    public bool IncludeSocialInsurance { get; set; }
    public decimal? SocialInsuranceEmployeeRate { get; set; }
    public decimal? SocialInsuranceEmployerRate { get; set; }
    public string? PaymentMethod { get; set; }
    public PayrollBankInfoPayload? BankInfo { get; set; }
    public List<PayrollAllowancePayload> Allowances { get; set; } = new();
    public List<PayrollDeductionPayload> Deductions { get; set; } = new();
    public string? Notes { get; set; }
    public decimal OvertimeMultiplier { get; set; }
    public List<EmployeePayslipHistoryItemDto> PayrollHistory { get; set; } = new();
}

public class EmployeeAttendanceDetailsDto
{
    public string? WorkShift { get; set; }
    public string? WorkDays { get; set; }
    public string? GracePeriod { get; set; }
    public string? MaxLatePerMonth { get; set; }
    public bool OvertimeEligible { get; set; }
    public string? AttendanceMethod { get; set; }
    public string? LateDeductionPolicy { get; set; }
    public string? AbsenceDeductionPolicy { get; set; }
    public string? HalfDayRule { get; set; }
    public string? MissingCheckoutHandling { get; set; }
    public List<EmployeeAttendanceHistoryItemDto> AttendanceHistory { get; set; } = new();
}

public class EmployeeDocumentGroupDetailsDto
{
    public EmployeeDocumentType DocumentType { get; set; }
    public string DocumentTypeNameEn { get; set; } = string.Empty;
    public string DocumentTypeNameAr { get; set; } = string.Empty;
    public List<EmployeeDocumentAttachmentDetailsDto> Attachments { get; set; } = new();
}

public class EmployeeDocumentAttachmentDetailsDto
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
}

public class EmployeeAssetDetailsDto
{
    public Guid Id { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public string? ReturnNotes { get; set; }
    public string? Condition { get; set; }
    public decimal? Value { get; set; }
    public string? ImageUrl { get; set; }
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
    public string Status { get; set; } = string.Empty;
    public DateTime? GeneratedDate { get; set; }
    public DateTime? PaidDate { get; set; }
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
    public TimeSpan? OvertimeHours { get; set; }
    public bool IsLate { get; set; }
}

public class EmployeeBalanceSnapshotDto
{
    public int VacationYear { get; set; }
    public IReadOnlyCollection<EmployeeVacationBalanceSummaryDto> VacationBalances { get; set; } = Array.Empty<EmployeeVacationBalanceSummaryDto>();
    public int PermissionYear { get; set; }
    public int PermissionMonth { get; set; }
    public IReadOnlyCollection<EmployeePermissionBalanceSummaryDto> PermissionBalances { get; set; } = Array.Empty<EmployeePermissionBalanceSummaryDto>();
}

public class EmployeeVacationBalanceSummaryDto
{
    public Guid VacationTypeId { get; set; }
    public string VacationTypeNameEn { get; set; } = string.Empty;
    public string VacationTypeNameAr { get; set; } = string.Empty;
    public decimal AllocatedDays { get; set; }
    public decimal CarryOverDays { get; set; }
    public decimal ManualAdjustmentDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal AvailableDays { get; set; }
    public string? Notes { get; set; }
}

public class EmployeePermissionBalanceSummaryDto
{
    public Guid PermissionTypeId { get; set; }
    public string PermissionTypeNameEn { get; set; } = string.Empty;
    public string PermissionTypeNameAr { get; set; } = string.Empty;
    public decimal? MaxHoursPerMonth { get; set; }
    public decimal UsedHoursThisMonth { get; set; }
    public decimal? RemainingHoursThisMonth { get; set; }
    public string? Notes { get; set; }
}

public class EmployeeRoleDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }
}
