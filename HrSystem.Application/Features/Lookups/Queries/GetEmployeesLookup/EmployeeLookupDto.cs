namespace HrSystem.Application.Features.Lookups.Queries.GetEmployeesLookup;

public class EmployeeLookupDto
{
    public Guid Id { get; set; }
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
}
