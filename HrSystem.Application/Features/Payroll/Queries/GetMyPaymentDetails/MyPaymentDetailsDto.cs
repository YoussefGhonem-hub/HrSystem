namespace HrSystem.Application.Features.Payroll.Queries.GetMyPaymentDetails;

public class MyPaymentDetailsDto
{
    public string? PaymentMethod { get; set; }
    public string? BankName { get; set; }
    public string? IbanMasked { get; set; }
    public string? AccountHolderName { get; set; }
    public string? SalaryPaymentDay { get; set; } // e.g., "25th of every month"
}
