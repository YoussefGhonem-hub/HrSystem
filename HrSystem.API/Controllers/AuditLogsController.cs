using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.AuditLogs.Queries.GetAuditLogsList;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

/// <summary>
/// Audit trail for employees, attendance records and organization settings.
/// </summary>
[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin + "," + RoleNames.HRManager)]
public class AuditLogsController : APIBaseController
{
    private readonly ISender _mediator;

    public AuditLogsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Paged audit log. Filters: module (Employees | Attendance | OrganizationSettings),
    /// entityName, entityId, action (Created | Updated | Deleted | Restored), userId, date range, search.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? module = null,
        [FromQuery] string? entityName = null,
        [FromQuery] Guid? entityId = null,
        [FromQuery] string? action = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetAuditLogsListQuery(
            pageNumber, pageSize, module, entityName, entityId, action, userId, fromDate, toDate, searchTerm));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Full change history of a single record (e.g. one employee or one attendance row).
    /// </summary>
    [HttpGet("entity/{entityId:guid}")]
    public async Task<IActionResult> GetEntityHistory(
        Guid entityId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetAuditLogsListQuery(pageNumber, pageSize, EntityId: entityId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
