using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Employee;

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
    public Gender Gender { get; set; }
    public MaritalStatus MaritalStatus { get; set; }
    
    // Contact Information
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string AddressAr { get; set; } = string.Empty;
    public string? AddressEn { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "Egypt";
    
    // Employment Information
    public Guid DepartmentId { get; set; }
    public Guid JobTitleId { get; set; }
    public Guid? DirectManagerId { get; set; }
    public ContractType ContractType { get; set; }
    public EmployeeStatus Status { get; set; }
    
    public DateTime HiringDate { get; set; }
    public DateTime? ProbationEndDate { get; set; }
    public int ProbationPeriodMonths { get; set; } = 3;
    public DateTime? TerminationDate { get; set; }
    public string? TerminationReason { get; set; }
    
    // System Access
    public Guid? UserId { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Navigation Properties
    public virtual Department Department { get; set; } = null!;
    public virtual JobTitle JobTitle { get; set; } = null!;
    public virtual Employee? DirectManager { get; set; }
    public virtual ICollection<Employee> Subordinates { get; set; } = new List<Employee>();
    public virtual ICollection<EmployeeDocument> Documents { get; set; } = new List<EmployeeDocument>();
    public virtual ICollection<Payroll.Salary> Salaries { get; set; } = new List<Payroll.Salary>();
    public virtual ICollection<Attendance.Attendance> Attendances { get; set; } = new List<Attendance.Attendance>();
    public virtual ICollection<Leave.LeaveRequest> LeaveRequests { get; set; } = new List<Leave.LeaveRequest>();
    public virtual ICollection<Leave.LeaveBalance> LeaveBalances { get; set; } = new List<Leave.LeaveBalance>();
    public virtual ICollection<Performance.PerformanceReview> PerformanceReviews { get; set; } = new List<Performance.PerformanceReview>();
    public virtual ICollection<Lifecycle.EmployeeAsset> Assets { get; set; } = new List<Lifecycle.EmployeeAsset>();
}
