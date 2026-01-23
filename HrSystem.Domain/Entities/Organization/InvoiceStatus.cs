using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

public class InvoiceStatus : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? ColorCode { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation Properties
    public virtual ICollection<OrganizationInvoice> Invoices { get; set; } = new List<OrganizationInvoice>();
}
