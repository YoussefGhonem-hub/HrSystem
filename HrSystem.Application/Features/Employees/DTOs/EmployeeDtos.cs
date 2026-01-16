using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.DTOs;

public record EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstNameAr { get; init; } = string.Empty;
    public string LastNameAr { get; init; } = string.Empty;
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string FullNameAr { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? MobileNumber { get; init; }
    public string AddressAr { get; init; } = string.Empty;
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public Guid DepartmentId { get; init; }
    public string DepartmentNameEn { get; init; } = string.Empty;
    public string DepartmentNameAr { get; init; } = string.Empty;
    public Guid JobTitleId { get; init; }
    public string JobTitleEn { get; init; } = string.Empty;
    public string JobTitleAr { get; init; } = string.Empty;
    public Guid? DirectManagerId { get; init; }
    public string? DirectManagerName { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public ContractType ContractType { get; init; }
    public EmployeeStatus Status { get; init; }
    public DateTime HiringDate { get; init; }
    public DateTime? ProbationEndDate { get; init; }
    public int ProbationPeriodMonths { get; init; }
    public DateTime? TerminationDate { get; init; }
    public string? TerminationReason { get; init; }
    public string? ProfilePictureUrl { get; init; }
    public DateTime CreatedDate { get; init; }
}

public record EmployeeListDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string FullNameAr { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string DepartmentNameEn { get; init; } = string.Empty;
    public string JobTitleEn { get; init; } = string.Empty;
    public string? BranchName { get; init; }
    public EmployeeStatus Status { get; init; }
    public DateTime HiringDate { get; init; }
}

public record CreateEmployeeDto
{
    public string EmployeeCode { get; init; } = string.Empty;
    public string FirstNameAr { get; init; } = string.Empty;
    public string LastNameAr { get; init; } = string.Empty;
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string NationalId { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? MobileNumber { get; init; }
    public string AddressAr { get; init; } = string.Empty;
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public Guid DepartmentId { get; init; }
    public Guid JobTitleId { get; init; }
    public Guid? DirectManagerId { get; init; }
    public Guid? BranchId { get; init; }
    public ContractType ContractType { get; init; }
    public DateTime HiringDate { get; init; }
    public int ProbationPeriodMonths { get; init; } = 3;
}

public record UpdateEmployeeDto
{
    public string FirstNameAr { get; init; } = string.Empty;
    public string LastNameAr { get; init; } = string.Empty;
    public string FirstNameEn { get; init; } = string.Empty;
    public string LastNameEn { get; init; } = string.Empty;
    public string? PassportNumber { get; init; }
    public MaritalStatus MaritalStatus { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? MobileNumber { get; init; }
    public string AddressAr { get; init; } = string.Empty;
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public Guid DepartmentId { get; init; }
    public Guid JobTitleId { get; init; }
    public Guid? DirectManagerId { get; init; }
    public Guid? BranchId { get; init; }
    public ContractType ContractType { get; init; }
    public EmployeeStatus Status { get; init; }
}
