using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Employee;

/// <summary>
/// Central entity representing an employee in the organization.
/// This core entity stores comprehensive employee information including personal details, employment status,
/// and organizational relationships. Serves as the foundation for all HR operations including payroll,
/// attendance, performance management, and leave tracking. Essential for maintaining accurate workforce
/// records, ensuring compliance with labor laws, and enabling effective people management.
/// </summary>
public class Employee : BaseAuditableEntity
{
    // Personal Information
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstNameAr { get; set; } = string.Empty;
    public string LastNameAr { get; set; } = string.Empty;
    public string FirstNameEn { get; set; } = string.Empty;
    public string LastNameEn { get; set; } = string.Empty;
    public string FullNameAr => $"{FirstNameAr} {LastNameAr}";
    public string FullNameEn => $"{FirstNameEn} {LastNameEn}";
    
    public string NationalId { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Guid GenderId { get; set; }
    public Guid MaritalStatusId { get; set; }
    
    // Contact Information
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string AddressAr { get; set; } = string.Empty;
    public string? AddressEn { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Egypt";
    
    // Employment Information
    public Guid? DepartmentId { get; set; }
    public Guid? JobTitleId { get; set; }
    public Guid? DirectManagerId { get; set; }
    public Guid? ContractTypeId { get; set; }
    public Guid StatusId { get; set; }
    
    public DateTime? HiringDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public int? ProbationPeriodMonths { get; set; }
    public DateTime? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
    
    // System Access
    public Guid? UserId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Navigation Properties
    public virtual Gender Gender { get; set; } = null!;
    public virtual MaritalStatus MaritalStatus { get; set; } = null!;
    public virtual ContractType ContractType { get; set; } = null!;
    public virtual EmployeeStatus Status { get; set; } = null!;
    public virtual Department Department { get; set; } = null!;
    public virtual JobTitle JobTitle { get; set; } = null!;
    public virtual Employee? DirectManager { get; set; }
    public virtual Organization.Branch? Branch { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public virtual ICollection<Payroll.Salary> Salaries { get; set; } = new List<Payroll.Salary>();
    public virtual ICollection<Attendance.Attendance> Attendances { get; set; } = new List<Attendance.Attendance>();
    public virtual ICollection<Attendance.EmployeeBiometric> Biometrics { get; set; } = new List<Attendance.EmployeeBiometric>();
    public virtual ICollection<Leave.LeaveRequest> LeaveRequests { get; set; } = new List<Leave.LeaveRequest>();
    public virtual ICollection<Leave.LeaveBalance> LeaveBalances { get; set; } = new List<Leave.LeaveBalance>();
    public virtual ICollection<Performance.PerformanceReview> PerformanceReviews { get; set; } = new List<Performance.PerformanceReview>();
    public virtual ICollection<Lifecycle.EmployeeAsset> Assets { get; set; } = new List<Lifecycle.EmployeeAsset>();
}
