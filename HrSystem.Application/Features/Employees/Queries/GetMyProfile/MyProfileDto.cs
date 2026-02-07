namespace HrSystem.Application.Features.Employees.Queries.GetMyProfile;

public class MyProfileDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public string? GenderNameEn { get; set; }
    public string? GenderNameAr { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string NationalIdNumber { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? PersonalEmail { get; set; }
    public string? WorkEmail { get; set; }
    public string? JobTitleEn { get; set; }
    public string? JobTitleAr { get; set; }
    public string? DepartmentNameEn { get; set; }
    public string? DepartmentNameAr { get; set; }
    public string? EmploymentStatusEn { get; set; }
    public string? EmploymentStatusAr { get; set; }
    public DateTime? HiringDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public string? MedicalInsuranceStatus { get; set; }
}
