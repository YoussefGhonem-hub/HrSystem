using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities.Requests;

/// <summary>
/// Determines which request types are visible/allowed for employees that belong to a given branch.
/// Acts as a feature toggle as well as a guardrail (max open requests, attachment requirements, etc.).
/// </summary>
public class BranchRequestSetting : BaseAuditableEntity
{
    public EmployeeRequestType RequestType { get; set; }
    public bool IsVisibleToEmployees { get; set; } = true;
    public bool AllowEmployeesToSubmit { get; set; } = true;
    public bool RequireAttachment { get; set; }
    public int? MaxOpenRequests { get; set; }
    public string? CustomInstructions { get; set; }

    public virtual Branch Branch { get; set; } = null!;
}
