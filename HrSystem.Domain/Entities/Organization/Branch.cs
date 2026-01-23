using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;

namespace HrSystem.Domain.Entities.Organization;

/// <summary>
/// Represents a physical branch or location of an organization.
/// This entity enables multi-branch management for organizations operating in multiple countries or cities.
/// Each branch can have its own departments, employees, settings, and operational parameters.
/// Critical for managing distributed workforces across different geographical locations with
/// country-specific regulations, currencies, and working hours.
/// </summary>
public class Branch : BaseAuditableEntity
{
    // Basic Information
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // Unique branch code (e.g., EG-01, KSA-01)
    public string? Description { get; set; }
    
    // Organization Reference
    public Guid OrganizationId { get; set; }
    
    // Location Information
    public Guid CountryId { get; set; }
    public string? City { get; set; }
    public string? AddressAr { get; set; }
    public string? AddressEn { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    // Contact Information
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Fax { get; set; }
    
    // Branch Settings
    public string TimeZone { get; set; } = "Egypt Standard Time";
    public string Currency { get; set; } = "EGP";
    public string? Language { get; set; } = "ar-EG";
    
    // Operational Information
    public bool IsHeadquarter { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? OpeningDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    
    // Branch Manager
    public Guid? BranchManagerId { get; set; }
    
    // Capacity
    public int? MaxEmployeeCapacity { get; set; }
    public int CurrentEmployeeCount { get; set; }
    
    // Working Hours (Optional - for future features)
    public TimeSpan? WorkStartTime { get; set; }
    public TimeSpan? WorkEndTime { get; set; }
    public string? WorkingDays { get; set; } // e.g., "Sunday,Monday,Tuesday,Wednesday,Thursday"
    
    // Navigation Properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Country Country { get; set; } = null!;
    public virtual Employee.Employee? BranchManager { get; set; }
    public virtual ICollection<Employee.Department> Departments { get; set; } = new List<Employee.Department>();
    public virtual ICollection<Employee.Employee> Employees { get; set; } = new List<Employee.Employee>();
    public virtual ICollection<UserBranchRole> UserBranchRoles { get; set; } = new List<UserBranchRole>();
}
