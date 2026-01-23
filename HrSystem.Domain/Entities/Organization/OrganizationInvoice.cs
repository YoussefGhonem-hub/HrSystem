using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Manages billing and payment records for organization subscriptions.
/// This entity tracks subscription invoices, payment status, and billing history for each organization.
/// Essential for revenue tracking, accounts receivable management, financial reporting, and ensuring
/// timely payments. Supports automated billing processes and helps maintain healthy cash flow.
/// </summary>
public class OrganizationInvoice : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    
    // Amounts
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    
    public Guid StatusId { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    
    // Billing Period
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }

    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual InvoiceStatus Status { get; set; } = null!;
    public virtual ICollection<OrganizationInvoiceItem> Items { get; set; } = new List<OrganizationInvoiceItem>();
}
