namespace HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;

public class KPIEvaluationListDto
{
    public Guid Id { get; set; }
    public Guid PerformanceReviewId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string KPINameEn { get; set; } = string.Empty;
    public string KPINameAr { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public decimal WeightedScore { get; set; }
    public DateTime CreatedDate { get; set; }
}
