using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Leave.Queries.GetLeaveStatuses;
using HrSystem.Application.Features.Leave.Queries.GetLeaveTypes;
using HrSystem.Application.Features.Lookups.Queries.GetAttendanceStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetContractTypes;
using HrSystem.Application.Features.Lookups.Queries.GetCountries;
using HrSystem.Application.Features.Lookups.Queries.GetEmployeeStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetGenders;
using HrSystem.Application.Features.Lookups.Queries.GetMaritalStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetPayrollStatuses;
using HrSystem.Application.Features.Performance.Queries.GetReviewStatuses;
using HrSystem.Application.Features.Performance.Queries.GetReviewTypes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

/// <summary>
/// Controller for all lookup/dropdown data endpoints.
/// Centralizes all dropdown queries for easy discovery and consistent access patterns.
/// </summary>
[Route("api/[controller]")]
[Authorize]
public class LookupsController : APIBaseController
{
    private readonly ISender _mediator;

    public LookupsController(ISender mediator)
    {
        _mediator = mediator;
    }

    #region Leave Module

    /// <summary>
    /// Get all active leave statuses for dropdown
    /// </summary>
    [HttpGet("leave-statuses")]
    public async Task<IActionResult> GetLeaveStatuses()
    {
        var result = await _mediator.Send(new GetLeaveStatusesQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active leave types for dropdown
    /// </summary>
    [HttpGet("leave-types")]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var result = await _mediator.Send(new GetLeaveTypesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Performance Module

    /// <summary>
    /// Get all active review types for dropdown
    /// </summary>
    [HttpGet("review-types")]
    public async Task<IActionResult> GetReviewTypes()
    {
        var result = await _mediator.Send(new GetReviewTypesQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active review statuses for dropdown
    /// </summary>
    [HttpGet("review-statuses")]
    public async Task<IActionResult> GetReviewStatuses()
    {
        var result = await _mediator.Send(new GetReviewStatusesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Attendance Module

    /// <summary>
    /// Get all active attendance statuses for dropdown
    /// </summary>
    [HttpGet("attendance-statuses")]
    public async Task<IActionResult> GetAttendanceStatuses()
    {
        var result = await _mediator.Send(new GetAttendanceStatusesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Employee Module

    /// <summary>
    /// Get all active contract types for dropdown
    /// </summary>
    [HttpGet("contract-types")]
    public async Task<IActionResult> GetContractTypes()
    {
        var result = await _mediator.Send(new GetContractTypesQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active genders for dropdown
    /// </summary>
    [HttpGet("genders")]
    public async Task<IActionResult> GetGenders()
    {
        var result = await _mediator.Send(new GetGendersQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active marital statuses for dropdown
    /// </summary>
    [HttpGet("marital-statuses")]
    public async Task<IActionResult> GetMaritalStatuses()
    {
        var result = await _mediator.Send(new GetMaritalStatusesQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active employee statuses for dropdown
    /// </summary>
    [HttpGet("employee-statuses")]
    public async Task<IActionResult> GetEmployeeStatuses()
    {
        var result = await _mediator.Send(new GetEmployeeStatusesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Payroll Module

    /// <summary>
    /// Get all active payroll statuses for dropdown
    /// </summary>
    [HttpGet("payroll-statuses")]
    public async Task<IActionResult> GetPayrollStatuses()
    {
        var result = await _mediator.Send(new GetPayrollStatusesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Organization Module

    /// <summary>
    /// Get all active countries for dropdown
    /// </summary>
    [HttpGet("countries")]
    public async Task<IActionResult> GetCountries()
    {
        var result = await _mediator.Send(new GetCountriesQuery());
        return result.Match(Ok, Problem);
    }

    #endregion
}
