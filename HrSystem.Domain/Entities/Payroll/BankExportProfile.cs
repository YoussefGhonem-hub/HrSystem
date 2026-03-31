using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Payroll;

public class BankExportProfile : BaseAuditableEntity
{
    public string ProfileName { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string CompanyAccountNumber { get; set; } = string.Empty;
    public string CompanyAccountName { get; set; } = string.Empty;
    public string Currency { get; set; } = "EGP";
    public string? BicCode { get; set; }
    public string Narrative { get; set; } = "Salary";
    public bool IsDefault { get; set; }

    // Template file stored in S3
    public string? TemplateFileKey { get; set; }
    public string? TemplateFileName { get; set; }
}
