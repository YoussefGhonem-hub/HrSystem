using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : APIBaseController
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="command">Login credentials (username and password)</param>
    /// <returns>JWT token with user information and roles</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
