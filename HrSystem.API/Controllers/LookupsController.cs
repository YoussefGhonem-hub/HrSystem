using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Lookups.Queries.GetAttendanceStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetContractTypes;
using HrSystem.Application.Features.Lookups.Queries.GetCountries;
using HrSystem.Application.Features.Lookups.Queries.GetBranchLookup;
using HrSystem.Application.Features.Lookups.Queries.GetEmployeeStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetGenders;
using HrSystem.Application.Features.Lookups.Queries.GetInvoiceStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetDirectManagersLookup;
using HrSystem.Application.Features.Lookups.Queries.GetMaritalStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetJobTitlesLookup;
using HrSystem.Application.Features.Lookups.Queries.GetPayrollStatuses;
using HrSystem.Application.Features.Lookups.Queries.GetRequestMasterLookups;
using HrSystem.Application.Features.Performance.Queries.GetGoalPriorities;
using HrSystem.Application.Features.Performance.Queries.GetGoalStatuses;
using HrSystem.Application.Features.Performance.Queries.GetReviewStatuses;
using HrSystem.Application.Features.Performance.Queries.GetReviewTypes;
using HrSystem.Application.Features.Lookups.Queries.GetEmployeesLookup;
using HrSystem.Application.Features.Lookups.Queries.GetDepartmentLookup;
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

    #region Request Master Lookups

    /// <summary>
    /// Get master request types (categories) for dropdowns
    /// </summary>
    [HttpGet("request-types")]
    public async Task<IActionResult> GetRequestTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetRequestTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get vacation request sub-types for dropdowns
    /// </summary>
    [HttpGet("vacation-types")]
    public async Task<IActionResult> GetVacationTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetVacationTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get training request sub-types for dropdowns
    /// </summary>
    [HttpGet("training-types")]
    public async Task<IActionResult> GetTrainingTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetTrainingTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get personal request sub-types for dropdowns
    /// </summary>
    [HttpGet("personal-types")]
    public async Task<IActionResult> GetPersonalTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetPersonalTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get permission request sub-types for dropdowns
    /// </summary>
    [HttpGet("permission-types")]
    public async Task<IActionResult> GetPermissionTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetPermissionTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get overtime request sub-types for dropdowns
    /// </summary>
    [HttpGet("overtime-types")]
    public async Task<IActionResult> GetOvertimeTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetOvertimeTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get miscellaneous request sub-types for dropdowns
    /// </summary>
    [HttpGet("miscellaneous-types")]
    public async Task<IActionResult> GetMiscellaneousTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetMiscellaneousTypesLookupQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get feedback request sub-types for dropdowns
    /// </summary>
    [HttpGet("feedback-types")]
    public async Task<IActionResult> GetFeedbackTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetFeedbackTypesLookupQuery(includeInactive));
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

    /// <summary>
    /// Get all active goal statuses for dropdown
    /// </summary>
    [HttpGet("goal-statuses")]
    public async Task<IActionResult> GetGoalStatuses()
    {
        var result = await _mediator.Send(new GetGoalStatusesQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all active goal priorities for dropdown
    /// </summary>
    [HttpGet("goal-priorities")]
    public async Task<IActionResult> GetGoalPriorities()
    {
        var result = await _mediator.Send(new GetGoalPrioritiesQuery());
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
    /// Get all active branches for dropdown
    /// </summary>
    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches()
    {
        var result = await _mediator.Send(new GetBranchLookupQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all departments (non-paginated) for dropdown
    /// </summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var result = await _mediator.Send(new GetDepartmentLookupQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all job titles (non-paginated) for dropdown
    /// </summary>
    [HttpGet("job-titles")]
    public async Task<IActionResult> GetJobTitles()
    {
        var result = await _mediator.Send(new GetJobTitlesLookupQuery());
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
    /// Get all active invoice statuses for dropdown
    /// </summary>
    [HttpGet("invoice-statuses")]
    public async Task<IActionResult> GetInvoiceStatuses()
    {
        var result = await _mediator.Send(new GetInvoiceStatusesQuery());
        return result.Match(Ok, Problem);
    }

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

    #region Employees Lookup

    /// <summary>
    /// Get all employees (non-paginated) for dropdowns
    /// </summary>
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var result = await _mediator.Send(new GetEmployeesLookupQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all direct managers (non-paginated) for dropdowns
    /// </summary>
    [HttpGet("directmanagers")]
    public async Task<IActionResult> GetDirectManagers()
    {
        var result = await _mediator.Send(new GetDirectManagersLookupQuery());
        return result.Match(Ok, Problem);
    }

    #endregion
}
