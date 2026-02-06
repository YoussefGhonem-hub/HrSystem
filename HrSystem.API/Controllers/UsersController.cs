using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Users.Commands.ActivateAccount;
using HrSystem.Application.Features.Users.Commands.ChangeUserRole;
using HrSystem.Application.Features.Users.Commands.CreateUserWithBranchRoles;
using HrSystem.Application.Features.Users.Commands.DeactivateAccount;
using HrSystem.Application.Features.Users.Commands.ResetUserPassword;
using HrSystem.Application.Features.Users.Commands.UpdateAccountSettings;
using HrSystem.Application.Features.Users.Commands.UpdateUser;
using HrSystem.Application.Features.Users.Queries.GetAccountSettings;
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

    /// <summary>
    /// Deactivate user account
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateAccount(Guid id)
    {
        var result = await _mediator.Send(new DeactivateAccountCommand(id));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Activate user account
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> ActivateAccount(Guid id)
    {
        var result = await _mediator.Send(new ActivateAccountCommand(id));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Change user roles for a specific branch
    /// </summary>
    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] ChangeUserRoleCommand command)
    {
        if (id != command.UserId)
        {
            return BadRequest("User ID mismatch");
        }

        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get account settings for a specific user
    /// </summary>
    [HttpGet("{id:guid}/account-settings")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin + "," + RoleNames.HRManager + "," + RoleNames.HRSpecialist)]
    public async Task<IActionResult> GetAccountSettings(Guid id)
    {
        var result = await _mediator.Send(new GetAccountSettingsQuery(id));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update account settings (username, email, status)
    /// </summary>
    [HttpPut("{id:guid}/account-settings")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin + "," + RoleNames.HRManager + "," + RoleNames.HRSpecialist)]
    public async Task<IActionResult> UpdateAccountSettings(Guid id, [FromBody] UpdateAccountSettingsCommand command)
    {
        if (id != command.UserId)
        {
            return BadRequest("User ID mismatch");
        }

        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Reset user password (Admin action)
    /// </summary>
    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin + "," + RoleNames.HRManager + "," + RoleNames.HRSpecialist)]
    public async Task<IActionResult> ResetUserPassword(Guid id, [FromBody] ResetUserPasswordCommand command)
    {
        if (id != command.UserId)
        {
            return BadRequest("User ID mismatch");
        }

        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
