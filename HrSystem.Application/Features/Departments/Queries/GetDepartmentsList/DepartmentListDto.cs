namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentsList;

public record DepartmentListDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? ManagerName { get; init; }
    public string? BranchName { get; init; }
    public int EmployeeCount { get; init; }
}
