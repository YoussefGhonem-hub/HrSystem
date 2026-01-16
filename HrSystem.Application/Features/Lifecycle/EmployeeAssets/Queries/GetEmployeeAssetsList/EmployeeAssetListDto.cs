namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;

public record EmployeeAssetListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string AssetType { get; init; } = string.Empty;
    public string AssetName { get; init; } = string.Empty;
    public string? SerialNumber { get; init; }
    public DateTime AssignedDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    public bool IsReturned { get; init; }
    public decimal? Value { get; init; }
    public string Condition { get; init; } = string.Empty;
}
