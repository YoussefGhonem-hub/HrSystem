using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApprovePermissionRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.ApproveVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateEmployeeRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateOvertimeRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreatePermissionRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateTrainingRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.CreateVacationRequest;
using HrSystem.Application.Features.EmployeeRequests.Commands.UpsertBranchRequestSettings;
using HrSystem.Application.Features.EmployeeRequests.Commands.Training;
using HrSystem.Application.Features.EmployeeRequests.Commands.Overtime;
using HrSystem.Application.Features.EmployeeRequests.Commands.Miscellaneous;
using HrSystem.Application.Features.EmployeeRequests.Commands.Personal;
using HrSystem.Application.Features.EmployeeRequests.Commands.Feedback;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetBranchAvailableRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetEmployeeRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.GetMyEmployeeRequests;
using HrSystem.Application.Features.EmployeeRequests.Queries.Permission;
using HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.Vacation;
using HrSystem.Application.Features.EmployeeRequests.Queries.Training;
using HrSystem.Application.Features.EmployeeRequests.Queries.Overtime;
using HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;
using HrSystem.Application.Features.EmployeeRequests.Queries.Personal;
using HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
    public async Task<IActionResult> GetMyRequests([FromQuery] string? requestTypeCode)
    {
        var employeeId = CurrentUser.EmployeeId;
        if (!employeeId.HasValue)
        {
            return BadRequest("The logged-in user is not linked to an employee profile.");
        }

        var result = await _mediator.Send(new GetMyEmployeeRequestsQuery(employeeId.Value, requestTypeCode));
        return result.Match(response => Ok(response), Problem);
    }

    /// <summary>
    /// Returns all requests for a specific employee with status statistics.
    /// Used by managers/HR to view an employee's requests.
    /// </summary>
    [Authorize(Roles = "OrganizationAdmin,HRManager,HRSpecialist,DepartmentManager")]
    [HttpGet("employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeRequests(
        Guid employeeId,
        [FromQuery] string? requestTypeCode,
        [FromQuery] EmployeeRequestStatus? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetEmployeeRequestsQuery(
            employeeId,
            requestTypeCode,
            status,
            pageNumber,
            pageSize));
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
            request.RequestTypeCode,
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
    /// Updates the request settings for a branch. Restricted to SuperAdmin.
    /// </summary>
    [Authorize(Roles = "SuperAdmin")]
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
    /// Submits a permission request (leave early, come late, short absence).
    /// This is hours-based and validates against monthly hour limits.
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
    /// Gets the employee's monthly permission hours usage for a specific permission type.
    /// Useful for showing remaining hours before submitting a permission request.
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

        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        var query = new GetEmployeeMonthlyPermissionHoursQuery(
            employeeId.Value,
            permissionTypeId,
            targetYear,
            targetMonth);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a vacation request.
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
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
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
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

    #region Vacation Requests - Role-Based Endpoints

    /// <summary>
    /// Get vacation requests based on current user's role
    /// - Employee: Gets all their own vacation requests
    /// - Department Manager: Gets pending requests from direct reports
    /// - HR Manager: Gets manager-approved requests waiting for HR approval
    /// </summary>
    [HttpGet("vacation")]
    public async Task<IActionResult> GetVacationRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? vacationTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetVacationRequestsQuery(
            status,
            vacationTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get vacation request details by ID
    /// </summary>
    [HttpGet("vacation/{id:guid}")]
    public async Task<IActionResult> GetVacationRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetVacationRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch vacation summary (counts by status)
    /// </summary>
    [HttpGet("vacation/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrVacationSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrVacationSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch vacation requests (all statuses)
    /// </summary>
    [HttpGet("vacation/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrVacationRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? vacationTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrVacationRequestsQuery(
            status,
            vacationTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own vacation requests + pending approvals from direct reports
    /// </summary>
    [HttpGet("vacation/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerVacationOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerVacationOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Explicit endpoint for employees to get their own vacation requests
    /// </summary>
    [HttpGet("vacation/my-requests")]
    public async Task<IActionResult> GetMyVacationRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? vacationTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetVacationRequestsQuery(
            status,
            vacationTypeId,
            startDateFrom,
            startDateTo,
            null,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize));

        return result.Match(Ok, Problem);
    }

    #endregion

    #region Permission Requests - Role-Based Endpoints

    /// <summary>
    /// Get permission requests based on current user's role
    /// - Employee: Gets all their own permission requests
    /// - Department Manager: Gets pending requests from direct reports
    /// - HR Manager: Gets manager-approved requests waiting for HR approval
    /// </summary>
    [HttpGet("permission/list")]
    public async Task<IActionResult> GetPermissionRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? permissionTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetPermissionRequestsQuery(
            status,
            permissionTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get permission request details by ID
    /// </summary>
    [HttpGet("permission/{id:guid}")]
    public async Task<IActionResult> GetPermissionRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetPermissionRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch permission summary (counts by status)
    /// </summary>
    [HttpGet("permission/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPermissionSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrPermissionSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch permission requests (all statuses)
    /// </summary>
    [HttpGet("permission/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPermissionRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? permissionTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrPermissionRequestsQuery(
            status,
            permissionTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own permission requests + pending approvals from direct reports
    /// </summary>
    [HttpGet("permission/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerPermissionOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerPermissionOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Explicit endpoint for employees to get their own permission requests
    /// </summary>
    [HttpGet("permission/my-requests")]
    public async Task<IActionResult> GetMyPermissionRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? permissionTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPermissionRequestsQuery(
            status,
            permissionTypeId,
            startDateFrom,
            startDateTo,
            null,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize));

        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a permission request.
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("permission/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApprovePermission(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApprovePermissionRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a permission request (after manager approval).
    /// May update employee's leave balance if the permission type deducts from leave.
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("permission/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApprovePermission(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApprovePermissionRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Training Requests - Role-Based Endpoints

    /// <summary>
    /// Get training requests based on current user's role
    /// </summary>
    [HttpGet("training/list")]
    public async Task<IActionResult> GetTrainingRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? trainingTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetTrainingRequestsQuery(
            status,
            trainingTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get training request details by ID
    /// </summary>
    [HttpGet("training/{id:guid}")]
    public async Task<IActionResult> GetTrainingRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainingRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch training summary
    /// </summary>
    [HttpGet("training/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrTrainingSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrTrainingSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch training requests
    /// </summary>
    [HttpGet("training/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrTrainingRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? trainingTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrTrainingRequestsQuery(
            status,
            trainingTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own training requests + pending approvals
    /// </summary>
    [HttpGet("training/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerTrainingOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerTrainingOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a training request
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("training/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApproveTraining(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveTrainingRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a training request
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("training/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApproveTraining(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveTrainingRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Overtime Requests - Role-Based Endpoints

    /// <summary>
    /// Get overtime requests based on current user's role
    /// </summary>
    [HttpGet("overtime/list")]
    public async Task<IActionResult> GetOvertimeRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? overtimeTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetOvertimeRequestsQuery(
            status,
            overtimeTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get overtime request details by ID
    /// </summary>
    [HttpGet("overtime/{id:guid}")]
    public async Task<IActionResult> GetOvertimeRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetOvertimeRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch overtime summary
    /// </summary>
    [HttpGet("overtime/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrOvertimeSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrOvertimeSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch overtime requests
    /// </summary>
    [HttpGet("overtime/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrOvertimeRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? overtimeTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrOvertimeRequestsQuery(
            status,
            overtimeTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own overtime requests + pending approvals
    /// </summary>
    [HttpGet("overtime/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerOvertimeOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerOvertimeOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects an overtime request
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("overtime/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApproveOvertime(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveOvertimeRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects an overtime request
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("overtime/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApproveOvertime(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveOvertimeRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Miscellaneous Requests - Role-Based Endpoints

    /// <summary>
    /// Get miscellaneous requests based on current user's role
    /// </summary>
    [HttpGet("miscellaneous/list")]
    public async Task<IActionResult> GetMiscellaneousRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? miscellaneousTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetMiscellaneousRequestsQuery(
            status,
            miscellaneousTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get miscellaneous request details by ID
    /// </summary>
    [HttpGet("miscellaneous/{id:guid}")]
    public async Task<IActionResult> GetMiscellaneousRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetMiscellaneousRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch miscellaneous summary
    /// </summary>
    [HttpGet("miscellaneous/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrMiscellaneousSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrMiscellaneousSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch miscellaneous requests
    /// </summary>
    [HttpGet("miscellaneous/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrMiscellaneousRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? miscellaneousTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrMiscellaneousRequestsQuery(
            status,
            miscellaneousTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own miscellaneous requests + pending approvals
    /// </summary>
    [HttpGet("miscellaneous/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerMiscellaneousOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerMiscellaneousOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a miscellaneous request
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("miscellaneous/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApproveMiscellaneous(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveMiscellaneousRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a miscellaneous request
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("miscellaneous/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApproveMiscellaneous(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveMiscellaneousRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Personal Requests - Role-Based Endpoints

    /// <summary>
    /// Get personal requests based on current user's role
    /// </summary>
    [HttpGet("personal/list")]
    public async Task<IActionResult> GetPersonalRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? personalTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetPersonalRequestsQuery(
            status,
            personalTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get personal request details by ID
    /// </summary>
    [HttpGet("personal/{id:guid}")]
    public async Task<IActionResult> GetPersonalRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetPersonalRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch personal summary
    /// </summary>
    [HttpGet("personal/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPersonalSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrPersonalSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch personal requests
    /// </summary>
    [HttpGet("personal/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrPersonalRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? personalTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrPersonalRequestsQuery(
            status,
            personalTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own personal requests + pending approvals
    /// </summary>
    [HttpGet("personal/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerPersonalOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerPersonalOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a personal request
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("personal/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApprovePersonal(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApprovePersonalRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a personal request
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("personal/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApprovePersonal(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApprovePersonalRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Feedback Requests - Role-Based Endpoints

    /// <summary>
    /// Get feedback requests based on current user's role
    /// </summary>
    [HttpGet("feedback/list")]
    public async Task<IActionResult> GetFeedbackRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? feedbackTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetFeedbackRequestsQuery(
            status,
            feedbackTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get feedback request details by ID
    /// </summary>
    [HttpGet("feedback/{id:guid}")]
    public async Task<IActionResult> GetFeedbackRequestById(Guid id)
    {
        var result = await _mediator.Send(new GetFeedbackRequestByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch feedback summary
    /// </summary>
    [HttpGet("feedback/hr/summary")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrFeedbackSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
    {
        var result = await _mediator.Send(new GetHrFeedbackSummaryQuery(startDateFrom, startDateTo));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get HR branch feedback requests
    /// </summary>
    [HttpGet("feedback/hr/requests")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
    public async Task<IActionResult> GetHrFeedbackRequests(
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] Guid? feedbackTypeId = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetHrFeedbackRequestsQuery(
            status,
            feedbackTypeId,
            startDateFrom,
            startDateTo,
            employeeId,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager overview: own feedback requests + pending approvals
    /// </summary>
    [HttpGet("feedback/manager/overview")]
    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
    public async Task<IActionResult> GetManagerFeedbackOverview(
        [FromQuery] int myPageNumber = 1,
        [FromQuery] int myPageSize = 10,
        [FromQuery] int pendingPageNumber = 1,
        [FromQuery] int pendingPageSize = 10)
    {
        var result = await _mediator.Send(new GetManagerFeedbackOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Manager approves or rejects a feedback request
    /// </summary>
    [Authorize(Roles = "DepartmentManager,HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("feedback/{requestId:guid}/manager-approval")]
    public async Task<IActionResult> ManagerApproveFeedback(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveFeedbackRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.Manager);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// HR approves or rejects a feedback request
    /// </summary>
    [Authorize(Roles = "HRManager,HRSpecialist,OrganizationAdmin")]
    [HttpPost("feedback/{requestId:guid}/hr-approval")]
    public async Task<IActionResult> HRApproveFeedback(
        Guid requestId,
        [FromBody] ApprovalDto approval)
    {
        var command = new ApproveFeedbackRequestCommand(
            requestId,
            approval.IsApproved,
            approval.Comments,
            ApprovalLevel.HR);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    #endregion

    public record ApprovalDto
    {
        public bool IsApproved { get; init; }
        public string? Comments { get; init; }
    }

    public record SubmitEmployeeRequestDto
    {
        public string RequestTypeCode { get; init; } = string.Empty;
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

    public record TrainingApprovalDto
    {
        public bool IsApproved { get; init; }
        public string? RejectionReason { get; init; }
        public decimal? ApprovedBudget { get; init; }
        public string? ApprovalNotes { get; init; }
    }

    public record OvertimeApprovalDto
    {
        public bool IsApproved { get; init; }
        public string? RejectionReason { get; init; }
        public decimal? ActualHours { get; init; }
        public string? ApprovalNotes { get; init; }
    }

    public record FeedbackApprovalDto
    {
        public bool IsApproved { get; init; }
        public string? RejectionReason { get; init; }
        public string? ResponseContent { get; init; }
    }
}
