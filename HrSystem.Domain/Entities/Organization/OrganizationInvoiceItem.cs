using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

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
