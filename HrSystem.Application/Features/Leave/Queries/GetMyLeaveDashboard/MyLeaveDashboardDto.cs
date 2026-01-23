namespace HrSystem.Application.Features.Leave.Queries.GetMyLeaveDashboard;

public class MyLeaveDashboardDto
{
    public int Year { get; set; }
    public decimal RemainingBalance { get; set; }
    public decimal CarryOverBalance { get; set; }
    public decimal AnnualLeaveBalance { get; set; }
    public decimal ConsumedDays { get; set; }
    public decimal CasualLeaveBalance { get; set; }
    public LeaveAccrualRulesDto? AnnualAccrualRules { get; set; }
    public SickLeaveDocumentRequirementDto? SickLeaveRequiredDocuments { get; set; }
    public List<LeaveRequestHistoryDto> RequestsHistory { get; set; } = new();
}

public class LeaveAccrualRulesDto
{
    public Guid LeavePolicyId { get; set; }
    public string PolicyNameEn { get; set; } = string.Empty;
    public string PolicyNameAr { get; set; } = string.Empty;
    public int DefaultDaysPerYear { get; set; }
    public int MaxCarryForward { get; set; }
    public int MaxConsecutiveDays { get; set; }
    public int MinDaysNotice { get; set; }
    public bool RequiresApproval { get; set; }
    public bool RequiresManagerApproval { get; set; }
    public bool RequiresHRApproval { get; set; }
    public bool IsPaid { get; set; }
    public string? Description { get; set; }
}

public class SickLeaveDocumentRequirementDto
{
    public Guid LeavePolicyId { get; set; }
    public string PolicyNameEn { get; set; } = string.Empty;
    public string PolicyNameAr { get; set; } = string.Empty;
    public bool RequiresDocument { get; set; }
    public string? Description { get; set; }
}

public class LeaveRequestHistoryDto
{
    public Guid LeaveRequestId { get; set; }
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeNameEn { get; set; } = string.Empty;
    public string LeaveTypeNameAr { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string StatusNameEn { get; set; } = string.Empty;
    public string StatusNameAr { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
}
