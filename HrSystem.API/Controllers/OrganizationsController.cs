using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationWithAdmin;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationsList;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationDetails;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class OrganizationsController : APIBaseController
{
    private readonly ISender _mediator;

    public OrganizationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new organization with branches and an OrganizationAdmin user
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationWithAdminCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(CreateOrganization), new { id = response.Data!.OrganizationId }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get all organizations (paginated). Only SuperAdmin.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetOrganizationsListQuery(pageNumber, pageSize, searchTerm));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get organization details by id. Only SuperAdmin.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganizationById(Guid id)
    {
        var result = await _mediator.Send(new GetOrganizationDetailsQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
