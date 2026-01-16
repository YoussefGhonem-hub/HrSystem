namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;

public class KPIListDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Weight { get; set; }
    public string? JobTitleEn { get; set; }
    public string? JobTitleAr { get; set; }
    public string? DepartmentNameEn { get; set; }
    public string? DepartmentNameAr { get; set; }
    public DateTime CreatedDate { get; set; }
}
