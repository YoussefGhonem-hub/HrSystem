using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchesList;

public record BranchListDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public Guid CountryId { get; init; }
    public string? CountryName { get; init; }
    public string? City { get; init; }
    public bool IsHeadquarter { get; init; }
    public bool IsActive { get; init; }
    public int EmployeeCount { get; init; }
}
