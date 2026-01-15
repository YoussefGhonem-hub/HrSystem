using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Represents individual line items within an organization invoice.
/// This entity provides detailed breakdown of charges including subscription fees, additional services,
/// or usage-based charges. Enables transparent billing, facilitates dispute resolution, and supports
/// detailed financial analysis of revenue sources.
/// </summary>
public class OrganizationInvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    // Navigation Properties
    public virtual OrganizationInvoice Invoice { get; set; } = null!;
}
