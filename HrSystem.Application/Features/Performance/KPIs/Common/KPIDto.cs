namespace HrSystem.Application.Features.Performance.KPIs.Common;

public class KPIDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Weight { get; set; }
    public string MeasurementCriteria { get; set; } = string.Empty;
    public Guid? JobTitleId { get; set; }
    public string? JobTitleEn { get; set; }
    public string? JobTitleAr { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentNameEn { get; set; }
    public string? DepartmentNameAr { get; set; }
    public DateTime CreatedDate { get; set; }
}
