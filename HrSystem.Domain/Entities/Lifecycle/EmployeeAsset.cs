using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Lifecycle;

public class EmployeeAsset : BaseAuditableEntity
{
    public Guid EmployeeId { get; set; }
    public string AssetType { get; set; } = string.Empty; // Laptop, Phone, ID Card, Access Card, etc.
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Model { get; set; }
    public string? Description { get; set; }
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public string Condition { get; set; } = "Good"; // Good, Fair, Poor, Damaged
    public string? ReturnNotes { get; set; }
    public decimal? Value { get; set; }

    // Navigation Properties
    public virtual Employee.Employee Employee { get; set; } = null!;
}
