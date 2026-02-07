using System;
using System.Collections.Generic;
using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequestLimits.Commands.UpsertEmployeeRequestLimits;
using HrSystem.Application.Features.EmployeeRequestLimits.Queries.GetEmployeeRequestLimits;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Authorize(Roles = $"{RoleNames.OrganizationAdmin},{RoleNames.HRManager},{RoleNames.HRSpecialist}")]
[Route("api/employee-request-limits")]
public class EmployeeRequestLimitsController : APIBaseController
{
    private readonly ISender _mediator;

    public EmployeeRequestLimitsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upserts per-employee limits for vacation days and permission hours.
    /// </summary>
    [HttpPost("{employeeId:guid}")]
    public async Task<IActionResult> UpsertRequestLimits(
        Guid employeeId,
        [FromBody] UpsertEmployeeRequestLimitsRequest request)
    {
        if (request is null)
        {
            return BadRequest("Request body is required.");
        }

        var command = new UpsertEmployeeRequestLimitsCommand(
            employeeId,
            request.VacationLimits,
            request.PermissionLimits);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves the configured per-employee request limits.
    /// </summary>
    [HttpGet("{employeeId:guid}")]
    public async Task<IActionResult> GetRequestLimits(Guid employeeId)
    {
        var query = new GetEmployeeRequestLimitsQuery(employeeId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    public class UpsertEmployeeRequestLimitsRequest
    {
        public IReadOnlyCollection<VacationLimitPayload>? VacationLimits { get; set; }
        public IReadOnlyCollection<PermissionLimitPayload>? PermissionLimits { get; set; }
    }
}
