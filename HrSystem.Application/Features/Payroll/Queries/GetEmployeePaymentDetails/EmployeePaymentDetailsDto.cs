namespace HrSystem.Application.Features.Payroll.Queries.GetEmployeePaymentDetails;

public class EmployeePaymentDetailsDto
{
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? PaymentMethod { get; set; }
    public string? BankName { get; set; }
    public string? IbanMasked { get; set; }
    public string? AccountHolderName { get; set; }
    public string? SalaryPaymentDay { get; set; }
}
