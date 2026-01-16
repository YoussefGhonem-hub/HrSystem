namespace HrSystem.Application.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public Guid? BranchId { get; init; }
}
