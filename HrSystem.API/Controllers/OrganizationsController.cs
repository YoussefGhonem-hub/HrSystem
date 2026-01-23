using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationWithAdmin;
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
}
