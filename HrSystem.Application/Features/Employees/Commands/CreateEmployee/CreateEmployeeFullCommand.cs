using System;
using System.Collections.Generic;
using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace HrSystem.Application.Features.Employees.Commands.CreateEmployee;

public class CreateEmployeeFullCommand : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>
{
    public CreateEmployeePersonalInfoSection PersonalInfo { get; set; } = null!;
    public CreateEmployeeJobInfoSection JobInfo { get; set; } = null!;
    public CreateEmployeePayrollSection Payroll { get; set; } = null!;
    public CreateEmployeeAttendanceSection Attendance { get; set; } = null!;
    public CreateEmployeeLeavesSection? Leaves { get; set; }
    public CreateEmployeeDocumentsSection? Documents { get; set; }
    public CreateEmployeeAssetsSection? Assets { get; set; }
}

public class CreateEmployeePersonalInfoSection
{
    public string FirstNameAr { get; set; } = "";
    public string LastNameAr { get; set; } = "";
    public string FirstNameEn { get; set; } = "";
    public string LastNameEn { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Guid GenderId { get; set; }
    public Guid MaritalStatusId { get; set; }
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? MobileNumber { get; set; }
    public string AddressAr { get; set; } = "";
    public string? AddressEn { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}

public class CreateEmployeeJobInfoSection
{
    public Guid DepartmentId { get; set; }
    public Guid JobTitleId { get; set; }
    public Guid? DirectManagerId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid ContractTypeId { get; set; }
    public DateTime HiringDate { get; set; }
    public int ProbationPeriodMonths { get; set; }
    public Guid? RoleId { get; set; }
}

public class CreateEmployeePayrollSection
{
    public decimal BasicSalary { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Currency { get; set; }
    public bool IncludeSocialInsurance { get; set; }
    public decimal? SocialInsuranceEmployeeRate { get; set; }
    public decimal? SocialInsuranceEmployerRate { get; set; }
    public string? PaymentMethod { get; set; }
    public PayrollBankInfoPayload? BankInfo { get; set; }
    public List<PayrollAllowancePayload>? Allowances { get; set; }
    public List<PayrollDeductionPayload>? Deductions { get; set; }
    public string? Notes { get; set; }
}

public class CreateEmployeeAttendanceSection
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
}

public class CreateEmployeeLeavesSection
{
    public decimal? Annual { get; set; }
    public decimal? Sick { get; set; }
    public decimal? Emergency { get; set; }
    public decimal? Compensatory { get; set; }
}

public class CreateEmployeeDocumentsSection
{
    public List<CreateEmployeeDocumentTypeGroup> Types { get; set; } = new();
}

public class CreateEmployeeDocumentTypeGroup
{
    public EmployeeDocumentType DocumentType { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public class CreateEmployeeAssetsSection
{
    public List<CreateEmployeeAssetPayload> Assets { get; set; } = new();
}

public class CreateEmployeeAssetPayload
{
    public string AssetType { get; set; } = "";
    public string AssetName { get; set; } = "";
    public string Condition { get; set; } = "";
    public DateTime AssignedDate { get; set; }
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? ReturnNotes { get; set; }
    public decimal? Value { get; set; }
    public bool IsReturned { get; set; }
}
