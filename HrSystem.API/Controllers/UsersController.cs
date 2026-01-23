using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Users.Commands.CreateUserWithBranchRoles;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin)]
public class UsersController : APIBaseController
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create a user with roles scoped to specific branches
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserWithBranchRolesCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(CreateUser), new { id = response.Data!.UserId }, response),
            errors => Problem(errors)
        );
    }
}
