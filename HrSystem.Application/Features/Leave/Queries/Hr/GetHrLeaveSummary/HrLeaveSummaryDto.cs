namespace HrSystem.Application.Features.Leave.Queries.Hr.GetHrLeaveSummary;

public class HrLeaveSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int ManagerApproved { get; set; }
    public int HRApproved { get; set; }
    public int Approved { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
}
