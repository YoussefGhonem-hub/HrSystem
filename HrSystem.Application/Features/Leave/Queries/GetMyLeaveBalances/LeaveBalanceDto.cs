namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveBalances;

public class LeaveBalanceDto
{
    public Guid LeavePolicyId { get; set; }
    public string LeavePolicyNameEn { get; set; } = string.Empty;
    public string LeavePolicyNameAr { get; set; } = string.Empty;
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeNameEn { get; set; } = string.Empty;
    public string LeaveTypeNameAr { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal TotalDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
    public decimal CarriedForwardDays { get; set; }
}
