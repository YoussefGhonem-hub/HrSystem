namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;

public record PolicyAcknowledgmentListDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string PolicyName { get; init; } = string.Empty;
    public string PolicyVersion { get; init; } = string.Empty;
    public DateTime AcknowledgedDate { get; init; }
    public bool IsAcknowledged { get; init; }
}
