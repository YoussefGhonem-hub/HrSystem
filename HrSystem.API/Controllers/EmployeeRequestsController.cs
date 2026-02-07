using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CancelRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreatePermissionRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateTrainingRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.UpsertBranchRequestSettings;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetAllMyRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetBranchAvailableRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetEmployeeRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetPendingApprovalRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetRequestById;
using HrSystem.Application.Features.EmployeeRequests.Queries.Permission;
using HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;
using HrSystem.Application.Features.EmployeeRequests.Queries.Training;
using HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;
using HrSystem.Application.Features.EmployeeRequests.Queries.Personal;
using HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Authorize(Roles = "OrganizationAdmin,HRManager,HRSpecialist,DepartmentManager,Employee")]
[Route("api/[controller]")]
public class EmployeeRequestsController : APIBaseController
{
    private readonly ISender _mediator;

    public EmployeeRequestsController(ISender mediator)
    {
        _mediator = mediator;
    }

    #region Unified Queries

    /// <summary>
    /// Returns all requests submitted by the logged-in employee across every request type
    /// (Vacation, Permission, Training, Overtime, Miscellaneous, Personal, Feedback).
    /// Supports filtering by request type code, status, date range, and pagination.
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests(
        [FromQuery] string? requestTypeCode = null,
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var employeeId = CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("The logged-in user is not linked to an employee profile.");

        var query = new GetAllMyRequestsQuery(
            employeeId.Value,
            requestTypeCode,
            status,
            startDateFrom,
            startDateTo,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns requests waiting for the current user's approval, across all request types.
    /// - Department Manager: sees Pending requests from direct reports.
    /// - HR Manager / HR Specialist / Org Admin: sees ManagerApproved requests for their branch.
    /// Supports filtering by request type code, date range, sorting, and pagination.
    /// </summary>
    [Authorize(Roles = "OrganizationAdmin,HRManager,HRSpecialist,DepartmentManager")]
    [HttpGet("pending-approval")]
    public async Task<IActionResult> GetPendingApprovalRequests(
        [FromQuery] string? requestTypeCode = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPendingApprovalRequestsQuery(
            requestTypeCode,
            startDateFrom,
            startDateTo,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns all requests for a specific employee with status statistics and pagination.
    /// Used by managers and HR to review an employee's full request history.
    /// </summary>
    [Authorize(Roles = "OrganizationAdmin,HRManager,HRSpecialist,DepartmentManager")]
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeRequests(
        Guid employeeId,
        [FromQuery] string? requestTypeCode = null,
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetEmployeeRequestsQuery(
            employeeId, requestTypeCode, status, pageNumber, pageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns the request types that are enabled for the current user's branch
    /// (or a specified branch). Used to build the "New Request" form dropdown.
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRequests([FromQuery] Guid? branchId)
    {
        var resolvedBranchId = branchId ?? CurrentUser.BranchId;
        if (!resolvedBranchId.HasValue)
            return BadRequest("Unable to resolve branch context for the current user.");

        var result = await _mediator.Send(new GetBranchAvailableRequestsQuery(resolvedBranchId.Value));
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Submit Requests

    /// <summary>
    /// Submits a new vacation/leave request with type-specific details.
    /// Validates against branch settings, attachment requirements, open request limits,
    /// and the employee's annual vacation day limit.
    /// </summary>
    [HttpPost("vacation")]
    public async Task<IActionResult> SubmitVacationRequest([FromBody] SubmitVacationRequestDto request)
    {
        var employeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("Employee context is required.");

        var command = new CreateVacationRequestCommand(
            employeeId.Value,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.VacationTypeId,
            request.TotalDays,
            request.AttachmentUrl,
            request.EmergencyContactName,
            request.EmergencyContactPhone,
            request.BranchId ?? CurrentUser.BranchId);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Submits a new permission request (leave early, come late, short absence).
    /// Hours-based request that validates against the employee's monthly permission hours limit.
    /// </summary>
    [HttpPost("permission")]
    public async Task<IActionResult> SubmitPermissionRequest([FromBody] SubmitPermissionRequestDto request)
    {
        var employeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("Employee context is required.");

        var command = new CreatePermissionRequestCommand(
            employeeId.Value,
            request.Title,
            request.Description,
            request.PermissionDate,
            request.FromTime,
            request.ToTime,
            request.TotalHours,
            request.PermissionTypeId,
            request.Reason,
            request.AttachmentUrl,
            request.BranchId ?? CurrentUser.BranchId);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Submits a new training request with course details, cost estimate, and objectives.
    /// Validated against branch settings and attachment rules.
    /// </summary>
    [HttpPost("training")]
    public async Task<IActionResult> SubmitTrainingRequest([FromBody] SubmitTrainingRequestDto request)
    {
        var employeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("Employee context is required.");

        var command = new CreateTrainingRequestCommand(
            employeeId.Value,
            request.Title,
            request.Description,
            request.TrainingTypeId,
            request.TrainingName,
            request.TrainingProvider,
            request.TrainingLocation,
            request.TrainingStartDate,
            request.TrainingEndDate,
            request.DurationDays,
            request.EstimatedCost,
            request.Currency,
            request.Objectives,
            request.ExpectedOutcome,
            request.AttachmentUrl,
            request.BranchId ?? CurrentUser.BranchId);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Request Details

    /// <summary>
    /// Returns full details for any request (Vacation, Permission, Training, Overtime,
    /// Miscellaneous, Personal, Feedback) by its ID. The response includes the type-specific
    /// detail DTO populated automatically based on the request type.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Approval & Lifecycle

    /// <summary>
    /// Unified approval endpoint for any request type.
    /// The approval level (Manager → HR) is auto-detected from the request's current status
    /// and the caller's role:
    /// - Pending + Manager/Admin role → Manager approval
    /// - ManagerApproved + HR role → HR (final) approval
    /// Type-specific logic is applied automatically (e.g. leave balance deduction for vacations).
    /// Pass IsApproved = false to reject at either stage.
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("{requestId:guid}/approve")]
    public async Task<IActionResult> ApproveRequest(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveRequestCommand(requestId, approval.IsApproved, approval.Comments);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Allows the requesting employee to cancel their own request.
    /// Only requests in Draft or Pending status may be cancelled.
    /// Once a request has been manager-approved or fully approved, it cannot be cancelled.
    /// </summary>
    [HttpPost("{requestId:guid}/cancel")]
    public async Task<IActionResult> CancelRequest(
        Guid requestId,
        [FromBody] CancelDto? cancelDto)
    {
        var command = new CancelRequestCommand(requestId, cancelDto?.Reason);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Utility

    /// <summary>
    /// Returns the employee's monthly permission hours usage for a specific permission type.
    /// Shows how many hours have been used and what the limit is.
    /// Useful for UI widgets that display remaining hours before submitting.
    /// </summary>
    [HttpGet("permission/monthly-hours")]
    public async Task<IActionResult> GetMonthlyPermissionHours(
        [FromQuery] Guid permissionTypeId,
        [FromQuery] int? year,
        [FromQuery] int? month)
    {
        var employeeId = CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("Employee context is required.");

        var query = new GetEmployeeMonthlyPermissionHoursQuery(
            employeeId.Value,
            permissionTypeId,
            year ?? DateTime.UtcNow.Year,
            month ?? DateTime.UtcNow.Month);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level vacation status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("vacation/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrVacationSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrVacationSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level permission status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("permission/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPermissionSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrPermissionSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level training status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("training/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrTrainingSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrTrainingSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level miscellaneous status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("miscellaneous/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrMiscellaneousSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrMiscellaneousSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level personal request status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("personal/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPersonalSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrPersonalSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Returns HR-level feedback request status summary (count by status) for the branch.
    /// Used for dashboard widgets.
    /// </summary>
    [HttpGet("feedback/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrFeedbackSummary(
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrFeedbackSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Creates or updates the request settings for a branch (which types are enabled, limits, etc.).
    /// Restricted to SuperAdmin.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("branches/{branchId:guid}/settings")]
    public async Task<IActionResult> UpsertBranchSettings(
        Guid branchId,
        [FromBody] List<BranchRequestSettingPayload> settings)
    {
        var command = new UpsertBranchRequestSettingsCommand(branchId, settings);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region DTOs

    public record ApprovalDto
    {
        /// <summary>True to approve, false to reject.</summary>
        public bool IsApproved { get; init; }
        /// <summary>Optional comments from the approver.</summary>
        public string? Comments { get; init; }
    }

    public record CancelDto
    {
        /// <summary>Optional reason for cancelling the request.</summary>
        public string? Reason { get; init; }
    }

    public record SubmitVacationRequestDto
    {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public Guid VacationTypeId { get; init; }
        public decimal TotalDays { get; init; }
        public string? AttachmentUrl { get; init; }
        public string? EmergencyContactName { get; init; }
        public string? EmergencyContactPhone { get; init; }
        public Guid? EmployeeId { get; init; }
        public Guid? BranchId { get; init; }
    }

    public record SubmitPermissionRequestDto
    {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime PermissionDate { get; init; }
        public TimeSpan? FromTime { get; init; }
        public TimeSpan? ToTime { get; init; }
        public decimal TotalHours { get; init; }
        public Guid PermissionTypeId { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string? AttachmentUrl { get; init; }
        public Guid? EmployeeId { get; init; }
        public Guid? BranchId { get; init; }
    }

    public record SubmitTrainingRequestDto
    {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public Guid TrainingTypeId { get; init; }
        public string TrainingName { get; init; } = string.Empty;
        public string? TrainingProvider { get; init; }
        public string? TrainingLocation { get; init; }
        public DateTime TrainingStartDate { get; init; }
        public DateTime TrainingEndDate { get; init; }
        public int DurationDays { get; init; }
        public decimal? EstimatedCost { get; init; }
        public string? Currency { get; init; }
        public string? Objectives { get; init; }
        public string? ExpectedOutcome { get; init; }
        public string? AttachmentUrl { get; init; }
        public Guid? EmployeeId { get; init; }
        public Guid? BranchId { get; init; }
    }

    #endregion
}
