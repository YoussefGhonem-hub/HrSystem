namespace HrSystem.Application.Features.Performance.KPIEvaluations.Common;

public class KPIEvaluationDto
{
    public Guid Id { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid KPIId { get; set; }
    public string KPINameEn { get; set; } = string.Empty;
    public string KPINameAr { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal WeightedScore { get; set; }
    public string? Comments { get; set; }
    public string? Evidence { get; set; }
    public DateTime CreatedDate { get; set; }
}
