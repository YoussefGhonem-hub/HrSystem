using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateEmployeeRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateTrainingRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.UpsertBranchRequestSettings;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetBranchAvailableRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetMyEmployeeRequests;
using HrSystem.Domain.Enums;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace HrSystem.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class EmployeeRequestsController : APIBaseController
{
    private readonly ISender _mediator;

    public EmployeeRequestsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns the request types that are enabled for the user's branch (or the provided branchId).
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableRequests([FromQuery] Guid? branchId)
    {
        var resolvedBranchId = branchId ?? CurrentUser.BranchId;
        if (!resolvedBranchId.HasValue)
        {
            return BadRequest("Unable to resolve branch context for the current user.");
        }

        var result = await _mediator.Send(new GetBranchAvailableRequestsQuery(resolvedBranchId.Value));
        return result.Match(response => Ok(response), Problem);
    }

    /// <summary>
    /// Returns the current employee's submitted requests with optional filtering by type.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRequests([FromQuery] EmployeeRequestType? type)
    {
        var employeeId = CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
        {
            return BadRequest("The logged-in user is not linked to an employee profile.");
        }

        var result = await _mediator.Send(new GetMyEmployeeRequestsQuery(employeeId.Value, type));
        return result.Match(response => Ok(response), Problem);
    }

    /// <summary>
    /// Submits a new self-service request on behalf of the logged-in employee (or the provided employee Id for admins).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> SubmitRequest([FromBody] SubmitEmployeeRequestDto request)
    {
        var employeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
        {
            return BadRequest("Employee context is required to submit a request.");
        }

        var branchId = request.BranchId ?? CurrentUser.BranchId;
        var command = new CreateEmployeeRequestCommand(
            request.RequestType,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.AttachmentUrl,
            employeeId.Value,
            branchId,
            null, // VacationDetail
            null, // OvertimeDetail
            null, // TrainingDetail
            null, // MiscellaneousDetail
            null, // PersonalDetail
            null  // FeedbackDetail
        );

        var result = await _mediator.Send(command);
        return result.Match(response => Ok(response), Problem);
    }

    /// <summary>
    /// Updates the request settings for a branch. Restricted to administrative roles.
    /// </summary>
    [Authorize(Roles = "Admin,OrganizationAdmin,HRManager")]
    [HttpPut("branches/{branchId:guid}/settings")]
    public async Task<IActionResult> UpsertBranchSettings(
        Guid branchId,
        [FromBody] List<BranchRequestSettingPayload> settings)
    {
        var command = new UpsertBranchRequestSettingsCommand(branchId, settings);
        var result = await _mediator.Send(command);
        return result.Match(response => Ok(response), Problem);
    }

    /// <summary>
    /// Submits a vacation/leave request with type-specific details.
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
    /// Submits an overtime request with type-specific details.
    /// </summary>
    [HttpPost("overtime")]
    public async Task<IActionResult> SubmitOvertimeRequest([FromBody] SubmitOvertimeRequestDto request)
    {
        var employeeId = request.EmployeeId ?? CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
            return BadRequest("Employee context is required.");

        var command = new CreateOvertimeRequestCommand(
            employeeId.Value,
            request.Title,
            request.Description,
            request.OvertimeDate,
            request.PlannedHours,
            request.Multiplier,
            request.ProjectCode,
            request.TaskDescription,
            request.AttachmentUrl,
            request.BranchId ?? CurrentUser.BranchId);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Submits a training request with type-specific details.
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

    /// <summary>
    /// Manager approves or rejects a vacation request.
    /// </summary>
    [Authorize(Roles = "Manager,DepartmentManager,HRManager,Admin,OrganizationAdmin")]
    [HttpPost("vacation/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApproveVacation(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveVacationRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a vacation request (after manager approval).
    /// Also updates employee's leave balance when approved.
    /// </summary>
    [Authorize(Roles = "HRManager,Admin,OrganizationAdmin")]
    [HttpPost("vacation/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApproveVacation(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveVacationRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    public record ApprovalDto
    {
        public bool IsApproved { get; init; }
        public string? Comments { get; init; }
    }

    public record SubmitEmployeeRequestDto
    {
        public EmployeeRequestType RequestType { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? AttachmentUrl { get; init; }
        public Guid? EmployeeId { get; init; }
        public Guid? BranchId { get; init; }
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

    public record SubmitOvertimeRequestDto
    {
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public DateTime OvertimeDate { get; init; }
        public TimeSpan PlannedHours { get; init; }
        public decimal Multiplier { get; init; } = 1.5m;
        public string? ProjectCode { get; init; }
        public string? TaskDescription { get; init; }
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
}
