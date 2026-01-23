using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Queries.GetMyLoans;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;
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

    /// <summary>
    /// Get loans list for the currently logged-in user
    /// </summary>
    [HttpGet("my-loans")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyLoans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMyLoansQuery(pageNumber, pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get loan details for the currently logged-in user
    /// </summary>
    [HttpGet("my-loans/{loanId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyLoanDetails(Guid loanId)
    {
        var result = await _mediator.Send(new GetMyLoanDetailsQuery(loanId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payslips list for the currently logged-in user
    /// </summary>
    [HttpGet("my-payslips")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPayslips(
        [FromQuery] int? year = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMyPayslipsQuery(year, pageNumber, pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
