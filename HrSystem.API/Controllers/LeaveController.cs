//using HrSystem.API.Controllers.Shared;
//using HrSystem.Application.Features.Leave.Commands.CreateLeavePolicy;
//using HrSystem.Application.Features.Leave.Commands.CreateLeaveRequest;
//using HrSystem.Application.Features.Leave.Commands.ApproveLeaveRequest;
//using HrSystem.Application.Features.Leave.Commands.DeleteLeavePolicy;
//using HrSystem.Application.Features.Leave.Commands.RejectLeaveRequest;
//using HrSystem.Application.Features.Leave.Commands.UpdateLeavePolicy;
//using HrSystem.Application.Features.Leave.Queries.GetLeaveRequestById;
//using HrSystem.Application.Features.Leave.Queries.GetLeaveRequests;
//using HrSystem.Application.Features.Leave.Queries.GetMyLeaveDashboard;
//using HrSystem.Application.Features.Leave.Queries.GetMyLeaveBalances;
//using HrSystem.Application.Features.Leave.Queries.GetLeavePolicies;
//using HrSystem.Application.Features.Leave.Queries.GetLeavePolicyById;
//using HrSystem.Application.Features.Leave.Queries.Hr.GetHrLeaveSummary;
//using HrSystem.Application.Features.Leave.Queries.Hr.GetHrLeaveRequests;
//using HrSystem.Application.Features.Leave.Queries.Manager.GetManagerLeaveOverview;
//using HrSystem.Shared.Constants;
//using MediatR;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace HrSystem.API.Controllers;

//[Route("api/[controller]")]
//[Authorize]
//public class LeaveController : APIBaseController
//{
//    private readonly ISender _mediator;

//    public LeaveController(ISender mediator)
//    {
//        _mediator = mediator;
//    }

//    /// <summary>
//    /// Get leave requests based on current user's role
//    /// - Employee: Gets all their own leave requests
//    /// - Department Manager: Gets pending requests from direct reports
//    /// - HR Manager: Gets manager-approved requests waiting for HR approval
//    /// </summary>
//    [HttpGet]
//    public async Task<IActionResult> GetLeaveRequests(
//        [FromQuery] Guid? statusId = null,
//        [FromQuery] Guid? leaveTypeId = null,
//        [FromQuery] DateTime? startDateFrom = null,
//        [FromQuery] DateTime? startDateTo = null,
//        [FromQuery] Guid? employeeId = null,
//        [FromQuery] string? sortBy = null,
//        [FromQuery] bool sortDescending = false,
//        [FromQuery] int pageNumber = 1,
//        [FromQuery] int pageSize = 10)
//    {
//        var query = new GetLeaveRequestsQuery(
//            statusId,
//            leaveTypeId,
//            startDateFrom,
//            startDateTo,
//            employeeId,
//            sortBy,
//            sortDescending,
//            pageNumber,
//            pageSize);

//        var result = await _mediator.Send(query);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Alias: Leave requests history with filters and pagination
//    /// </summary>
//    [HttpGet("history")]
//    public async Task<IActionResult> GetLeaveRequestsHistory(
//        [FromQuery] Guid? statusId = null,
//        [FromQuery] Guid? leaveTypeId = null,
//        [FromQuery] DateTime? startDateFrom = null,
//        [FromQuery] DateTime? startDateTo = null,
//        [FromQuery] Guid? employeeId = null,
//        [FromQuery] string? sortBy = null,
//        [FromQuery] bool sortDescending = false,
//        [FromQuery] int pageNumber = 1,
//        [FromQuery] int pageSize = 10)
//    {
//        var query = new GetLeaveRequestsQuery(
//            statusId,
//            leaveTypeId,
//            startDateFrom,
//            startDateTo,
//            employeeId,
//            sortBy,
//            sortDescending,
//            pageNumber,
//            pageSize);

//        var result = await _mediator.Send(query);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Create a new leave request
//    /// </summary>
//    [HttpPost]
//    public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestCommand command)
//    {
//        var result = await _mediator.Send(command);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get leave request details by ID
//    /// </summary>
//    [HttpGet("{id:guid}")]
//    public async Task<IActionResult> GetLeaveRequestById(Guid id)
//    {
//        var result = await _mediator.Send(new GetLeaveRequestByIdQuery(id));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get annual leave dashboard data for the currently logged-in user
//    /// </summary>
//    [HttpGet("my-dashboard")]
//    public async Task<IActionResult> GetMyLeaveDashboard([FromQuery] int? year = null, [FromQuery] int historyCount = 5)
//    {
//        var result = await _mediator.Send(new GetMyLeaveDashboardQuery(year, historyCount));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get leave balances for the currently logged-in user
//    /// </summary>
//    [HttpGet("my-balances")]
//    public async Task<IActionResult> GetMyLeaveBalances([FromQuery] int? year = null)
//    {
//        var result = await _mediator.Send(new GetMyLeaveBalancesQuery(year));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get HR branch leave summary (counts by status)
//    /// </summary>
//    [HttpGet("hr/summary")]
//    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
//    public async Task<IActionResult> GetHrLeaveSummary([FromQuery] DateTime? startDateFrom = null, [FromQuery] DateTime? startDateTo = null)
//    {
//        var result = await _mediator.Send(new GetHrLeaveSummaryQuery(startDateFrom, startDateTo));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get HR branch leave requests (all statuses)
//    /// </summary>
//    [HttpGet("hr/requests")]
//    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
//    public async Task<IActionResult> GetHrLeaveRequests(
//        [FromQuery] Guid? statusId = null,
//        [FromQuery] Guid? leaveTypeId = null,
//        [FromQuery] DateTime? startDateFrom = null,
//        [FromQuery] DateTime? startDateTo = null,
//        [FromQuery] Guid? employeeId = null,
//        [FromQuery] string? sortBy = null,
//        [FromQuery] bool sortDescending = false,
//        [FromQuery] int pageNumber = 1,
//        [FromQuery] int pageSize = 10)
//    {
//        var query = new GetHrLeaveRequestsQuery(
//            statusId,
//            leaveTypeId,
//            startDateFrom,
//            startDateTo,
//            employeeId,
//            sortBy,
//            sortDescending,
//            pageNumber,
//            pageSize);

//        var result = await _mediator.Send(query);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Manager overview: own requests + pending approvals from direct reports
//    /// </summary>
//    [HttpGet("manager/overview")]
//    [Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.DepartmentManager}")]
//    public async Task<IActionResult> GetManagerOverview(
//        [FromQuery] int myPageNumber = 1,
//        [FromQuery] int myPageSize = 10,
//        [FromQuery] int pendingPageNumber = 1,
//        [FromQuery] int pendingPageSize = 10)
//    {
//        var result = await _mediator.Send(new GetManagerLeaveOverviewQuery(myPageNumber, myPageSize, pendingPageNumber, pendingPageSize));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Explicit endpoint for employees to get their own leave requests
//    /// </summary>
//    [HttpGet("my-requests")]
//    public async Task<IActionResult> GetMyLeaveRequests(
//        [FromQuery] Guid? statusId = null,
//        [FromQuery] Guid? leaveTypeId = null,
//        [FromQuery] DateTime? startDateFrom = null,
//        [FromQuery] DateTime? startDateTo = null,
//        [FromQuery] string? sortBy = null,
//        [FromQuery] bool sortDescending = false,
//        [FromQuery] int pageNumber = 1,
//        [FromQuery] int pageSize = 10)
//    {
//        var result = await _mediator.Send(new GetLeaveRequestsQuery(
//            statusId,
//            leaveTypeId,
//            startDateFrom,
//            startDateTo,
//            null,
//            sortBy,
//            sortDescending,
//            pageNumber,
//            pageSize));

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }


//    /// <summary>
//    /// Approve a leave request
//    /// Supports multi-level approval: Manager approval -> HR approval
//    /// </summary>
//    [HttpPost("{id:guid}/approve")]
//    public async Task<IActionResult> ApproveLeaveRequest(Guid id, [FromBody] ApproveLeaveRequestCommand command)
//    {
//        if (id != command.LeaveRequestId)
//        {
//            return BadRequest("ID mismatch");
//        }

//        var result = await _mediator.Send(command);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Reject a leave request
//    /// Can be rejected by direct manager or HR manager
//    /// </summary>
//    [HttpPost("{id:guid}/reject")]
//    public async Task<IActionResult> RejectLeaveRequest(Guid id, [FromBody] RejectLeaveRequestCommand command)
//    {
//        if (id != command.LeaveRequestId)
//        {
//            return BadRequest("ID mismatch");
//        }

//        var result = await _mediator.Send(command);

//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    #region Leave Policies (Balance Settings)

//    /// <summary>
//    /// Get all leave policies (balance settings)
//    /// </summary>
//    [HttpGet("policies")]
//    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin + "," + RoleNames.SuperAdmin)]
//    public async Task<IActionResult> GetLeavePolicies()
//    {
//        var result = await _mediator.Send(new GetLeavePoliciesQuery());
//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Get leave policy by ID
//    /// </summary>
//    [HttpGet("policies/{id:guid}")]
//    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin + "," + RoleNames.SuperAdmin)]
//    public async Task<IActionResult> GetLeavePolicyById(Guid id)
//    {
//        var result = await _mediator.Send(new GetLeavePolicyByIdQuery(id));
//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Create a new leave policy (balance settings)
//    /// </summary>
//    [HttpPost("policies")]
//    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin + "," + RoleNames.SuperAdmin)]
//    public async Task<IActionResult> CreateLeavePolicy([FromBody] CreateLeavePolicyCommand command)
//    {
//        var result = await _mediator.Send(command);
//        return result.Match(
//            response => CreatedAtAction(nameof(GetLeavePolicyById), new { id = response.Data!.Id }, response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Update an existing leave policy (balance settings)
//    /// </summary>
//    [HttpPut("policies/{id:guid}")]
//    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin + "," + RoleNames.SuperAdmin)]
//    public async Task<IActionResult> UpdateLeavePolicy(Guid id, [FromBody] UpdateLeavePolicyCommand command)
//    {
//        if (id != command.Id)
//        {
//            return BadRequest("ID mismatch");
//        }

//        var result = await _mediator.Send(command);
//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    /// <summary>
//    /// Delete a leave policy (balance settings)
//    /// </summary>
//    [HttpDelete("policies/{id:guid}")]
//    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.OrganizationAdmin + "," + RoleNames.SuperAdmin)]
//    public async Task<IActionResult> DeleteLeavePolicy(Guid id)
//    {
//        var result = await _mediator.Send(new DeleteLeavePolicyCommand(id));
//        return result.Match(
//            response => Ok(response),
//            errors => Problem(errors)
//        );
//    }

//    #endregion
//}
