using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities.Performance;

/// <summary>
/// Defines Key Performance Indicators for measuring employee performance.
/// This entity standardizes performance measurement criteria across roles and departments,
/// enables objective performance evaluation, ensures consistency in assessments, and links
/// individual performance to business outcomes. Critical for fair performance management,
/// identifying high performers, and making data-driven talent decisions.
/// </summary>
public class KPI : BaseAuditableEntity
{
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Category { get; set; } = string.Empty; // Quality, Productivity, Innovation, etc.
    public int Weight { get; set; } // Percentage weight in overall evaluation
    public string MeasurementCriteria { get; set; } = string.Empty;
    public Guid? JobTitleId { get; set; }
    public Guid? DepartmentId { get; set; }

    // Navigation Properties
    public virtual Employee.JobTitle? JobTitle { get; set; }
    public virtual Employee.Department? Department { get; set; }
    public virtual ICollection<KPIEvaluation> KPIEvaluations { get; set; } = new List<KPIEvaluation>();
}
