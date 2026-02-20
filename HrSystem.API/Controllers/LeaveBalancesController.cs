using System;
using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.LeaveBalances.Commands.UpsertEmployeeLeaveBalances;
using HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveHistory;
using HrSystem.Application.Features.LeaveBalances.Queries.GetEmployeeLeaveBalanceSummary;
using HrSystem.Application.Features.LeaveBalances.Queries.GetLeaveReport;
using HrSystem.Domain.Enums;
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

    /// <summary>
    /// Get leave report with statistics and a filterable/sortable grid of vacation requests.
    /// </summary>
    /// <param name="year">Filter by year (matches requests whose start or end date falls in this year)</param>
    /// <param name="employeeId">Filter by specific employee</param>
    /// <param name="departmentId">Filter by department</param>
    /// <param name="vacationTypeId">Filter by vacation type (annual, sick, etc.)</param>
    /// <param name="status">Filter by request status (Pending, Approved, Rejected, etc.)</param>
    /// <param name="searchTerm">Search by employee code or name</param>
    /// <param name="sortBy">Sort field (EmployeeCode, EmployeeName, Department, VacationType, StartDate, EndDate, TotalDays, Status, RequestedDate)</param>
    /// <param name="sortDescending">Sort descending (default: true)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet("report")]
    public async Task<IActionResult> GetLeaveReport(
        [FromQuery] int? year = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? vacationTypeId = null,
        [FromQuery] EmployeeRequestStatus? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = "RequestedDate",
        [FromQuery] bool sortDescending = true)
    {
        var query = new GetLeaveReportQuery(
            year,
            employeeId,
            departmentId,
            vacationTypeId,
            status,
            searchTerm,
            pageNumber,
            pageSize,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    public class UpsertLeaveBalancesRequest
    {
        public int Year { get; set; }
        public IReadOnlyCollection<LeaveBalanceAllocationPayload>? Allocations { get; set; }
    }
}
