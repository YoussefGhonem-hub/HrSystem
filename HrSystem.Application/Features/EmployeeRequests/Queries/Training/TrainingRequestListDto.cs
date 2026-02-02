using HrSystem.Domain.Enums;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Training;

public class TrainingRequestListDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNameAr { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string? BranchName { get; set; }
    
    public Guid TrainingTypeId { get; set; }
    public string? TrainingTypeName { get; set; }
    public string? TrainingTypeNameAr { get; set; }
    public string TrainingName { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public string? TrainingLocation { get; set; }
    public DateTime TrainingStartDate { get; set; }
    public DateTime TrainingEndDate { get; set; }
    public int DurationDays { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? ApprovedBudget { get; set; }
    public string? Currency { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public EmployeeRequestStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    
    public int CurrentApprovalLevel { get; set; }
}
