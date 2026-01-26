using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Users.Commands.CreateUserWithBranchRoles;
using HrSystem.Application.Features.Users.Commands.UpdateUser;
using HrSystem.Application.Features.Users.Queries.GetUsersList;
using HrSystem.Application.Features.Users.Queries.GetUserById;
using HrSystem.Domain.Entities.Account;
using HrSystem.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin)]
public class UsersController : APIBaseController
{
    private readonly ISender _mediator;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public UsersController(ISender mediator, RoleManager<ApplicationRole> roleManager)
    {
        _mediator = mediator;
        _roleManager = roleManager;
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

    /// <summary>
    /// Get a paginated list of users with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? roleId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetUsersListQuery(
            pageNumber,
            pageSize,
            searchTerm,
            isActive,
            branchId,
            roleId,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get user by id
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update user basic information
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
