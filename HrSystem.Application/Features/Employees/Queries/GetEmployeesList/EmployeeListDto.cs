using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeesList;

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
