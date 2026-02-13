using System;
using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.LeaveBalances.Commands.UpsertEmployeeLeaveBalances;
using HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveHistory;
using HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveBalanceSummary;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class LeaveBalancesController : APIBaseController
{
    private readonly ISender _mediator;

    public LeaveBalancesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upserts (creates or updates) the annual leave balance buckets for a specific employee and year.
    /// </summary>
    [HttpPost("{employeeId:guid}")]
    public async Task<IActionResult> UpsertLeaveBalances(Guid employeeId, [FromBody] UpsertLeaveBalancesRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var command = new UpsertEmployeeLeaveBalancesCommand(
            employeeId,
            request.Year,
            request.Allocations ?? Array.Empty<LeaveBalanceAllocationPayload>());

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves the leave balance history for an employee with optional filtering.
    /// </summary>
    [HttpGet("{employeeId:guid}")]
    public async Task<IActionResult> GetLeaveHistory(
        Guid employeeId,
        [FromQuery] Guid? vacationTypeId,
        [FromQuery] int? fromYear,
        [FromQuery] int? toYear,
        [FromQuery] bool includeTransactions = false)
    {
        var query = new GetEmployeeLeaveHistoryQuery(
            employeeId,
            vacationTypeId,
            fromYear,
            toYear,
            includeTransactions);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves the leave balance summary for an employee (remaining balance, carry over, consumed, etc.).
    /// </summary>
    [HttpGet("{employeeId:guid}/summary")]
    public async Task<IActionResult> GetLeaveBalanceSummary(
        Guid employeeId,
        [FromQuery] int? year = null)
    {
        var query = new GetEmployeeLeaveBalanceSummaryQuery(employeeId, year);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves the leave balance summary for the current logged-in employee.
    /// </summary>
    [HttpGet("my-summary")]
    [Authorize] // Any authenticated user can access their own balance
    public async Task<IActionResult> GetMyLeaveBalanceSummary([FromQuery] int? year = null)
    {
        var query = new GetEmployeeLeaveBalanceSummaryQuery(null, year);

        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    public class UpsertLeaveBalancesRequest
    {
        public int Year { get; set; }
        public IReadOnlyCollection<LeaveBalanceAllocationPayload>? Allocations { get; set; }
    }
}
