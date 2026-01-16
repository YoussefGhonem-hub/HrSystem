using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.Branches.Commands.CreateBranch;

public record CreateBranchDto
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Country Country { get; init; }
    public string? City { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? PostalCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Fax { get; init; }
    public string TimeZone { get; init; } = "Egypt Standard Time";
    public string Currency { get; init; } = "EGP";
    public string? Language { get; init; }
    public bool IsHeadquarter { get; init; }
    public DateTime? OpeningDate { get; init; }
    public Guid? BranchManagerId { get; init; }
}
