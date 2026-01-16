using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.Commands.UpdateEmployee;

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
