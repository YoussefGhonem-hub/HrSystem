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
    public List<LeaveRequestHistoryDto> History { get; set; } = new();
}

public class LeaveRequestHistoryDto
{
    public Guid LeaveRequestId { get; set; }
    public Guid LeavePolicyId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string StatusNameEn { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? ApprovedDate { get; set; }
    public string? ManagerComments { get; set; }
    public string? HRComments { get; set; }
}
