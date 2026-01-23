namespace HrSystem.Application.Features.Branches.Queries.GetBranchById;

public record BranchDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CountryId { get; init; }
    public string? CountryNameEn { get; init; }
    public string? CountryNameAr { get; init; }
    public string? City { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? PostalCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Fax { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string? Language { get; init; }
    public bool IsHeadquarter { get; init; }
    public bool IsActive { get; init; }
    public DateTime? OpeningDate { get; init; }
    public DateTime? ClosingDate { get; init; }
    public Guid? BranchManagerId { get; init; }
    public string? BranchManagerName { get; init; }
    public int EmployeeCount { get; init; }
    public int DepartmentCount { get; init; }
    public DateTime CreatedDate { get; init; }
}
