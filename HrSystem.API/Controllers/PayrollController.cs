using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PayrollController : APIBaseController
{
    private readonly ISender _mediator;

    public PayrollController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get gross and net salary for the currently logged-in user
    /// </summary>
    [HttpGet("my-salary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMySalary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetMySalarySummaryQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
