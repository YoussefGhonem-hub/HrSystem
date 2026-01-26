using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Queries.GetMyLoans;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;
using HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollSummary;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipsList;
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
    /// Get payroll payslip history with filters and pagination
    /// - HR/Admin can filter by employee; others restricted to own data
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPayslipsHistory(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPayslipsListQuery(
            employeeId,
            year,
            month,
            isPaid,
            fromDate,
            toDate,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get aggregated payroll summary (employees paid, gross, deductions, net)
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPayrollSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetPayrollSummaryQuery(year, month));

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

    /// <summary>
    /// Get payslip full details for the currently logged-in user
    /// </summary>
    [HttpGet("my-payslips/{payslipId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyPayslipDetails(Guid payslipId)
    {
        var result = await _mediator.Send(new GetMyPayslipDetailsQuery(payslipId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
