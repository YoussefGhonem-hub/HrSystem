namespace HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;

public record PolicyAcknowledgmentDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string? EmployeeName { get; init; }
    public string PolicyName { get; init; } = string.Empty;
    public string PolicyVersion { get; init; } = string.Empty;
    public DateTime AcknowledgedDate { get; init; }
    public bool IsAcknowledged { get; init; }
    public string? DocumentPath { get; init; }
    public string? DocumentUrl { get; init; }
    public string? EmployeeSignature { get; init; }
    public string? Notes { get; init; }
}
