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

public record CreateEmployeeFullCommand(
    CreateEmployeePersonalInfoSection PersonalInfo,
    CreateEmployeeJobInfoSection JobInfo,
    CreateEmployeePayrollSection Payroll,
    CreateEmployeeAttendanceSection Attendance,
    CreateEmployeeDocumentsSection? Documents,
    CreateEmployeeAssetsSection? Assets
) : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;

public record CreateEmployeePersonalInfoSection(
    string FirstNameAr,
    string LastNameAr,
    string FirstNameEn,
    string LastNameEn,
    string NationalId,
    string? PassportNumber,
    DateTime DateOfBirth,
    Guid GenderId,
    Guid MaritalStatusId,
    string Email,
    string PhoneNumber,
    string? MobileNumber,
    string AddressAr,
    string? AddressEn,
    string? City,
    string? Country
);

public record CreateEmployeeJobInfoSection(
    Guid DepartmentId,
    Guid JobTitleId,
    Guid? DirectManagerId,
    Guid? BranchId,
    Guid ContractTypeId,
    DateTime HiringDate,
    int ProbationPeriodMonths
);

public record CreateEmployeePayrollSection(
    decimal BasicSalary,
    DateTime EffectiveDate,
    string? Currency,
    bool IncludeSocialInsurance,
    decimal? SocialInsuranceEmployeeRate,
    decimal? SocialInsuranceEmployerRate,
    string? PaymentMethod,
    PayrollBankInfoPayload? BankInfo,
    List<PayrollAllowancePayload>? Allowances,
    List<PayrollDeductionPayload>? Deductions,
    string? Notes
);

public record CreateEmployeeAttendanceSection(
    string? WorkShift,
    string? WorkDays,
    string? GracePeriod,
    string? MaxLatePerMonth,
    bool OvertimeEligible,
    string? AttendanceMethod,
    string? LateDeductionPolicy,
    string? AbsenceDeductionPolicy,
    string? HalfDayRule,
    string? MissingCheckoutHandling
);

public record CreateEmployeeDocumentsSection
{
    public List<CreateEmployeeDocumentTypeGroup> Types { get; set; } = new();
}

public record CreateEmployeeDocumentTypeGroup
{
    public EmployeeDocumentType DocumentType { get; set; }
    public List<IFormFile>? Attachments { get; set; }
}

public record CreateEmployeeAssetsSection(
    List<CreateEmployeeAssetPayload> Assets
);

public record CreateEmployeeAssetPayload(
    string AssetType,
    string AssetName,
    string Condition,
    DateTime AssignedDate,
    string? SerialNumber,
    string? Model,
    string? Description,
    DateTime? ExpectedReturnDate,
    DateTime? ReturnDate,
    string? ReturnNotes,
    decimal? Value,
    bool IsReturned = false
);
