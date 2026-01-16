namespace HrSystem.Application.Features.Departments.Queries.GetDepartmentById;

public record DepartmentDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public string? ParentDepartmentName { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public int EmployeeCount { get; init; }
    public DateTime CreatedDate { get; init; }
}
