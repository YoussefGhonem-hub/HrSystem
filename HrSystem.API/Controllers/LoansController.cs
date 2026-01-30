using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Loans.Commands.CreateLoan;
using HrSystem.Application.Features.Payroll.Loans.Commands.DeleteLoan;
using HrSystem.Application.Features.Payroll.Loans.Commands.UpdateLoan;
using HrSystem.Application.Features.Payroll.Loans.Queries.GetLoanById;
using HrSystem.Application.Features.Payroll.Loans.Queries.GetLoansList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class LoansController : APIBaseController
{
    private readonly ISender _mediator;

    public LoansController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get loans list with paging and filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLoans(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetLoansListQuery(
            pageNumber,
            pageSize,
            employeeId,
            isActive,
            searchTerm,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get loan by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLoanById(Guid id)
    {
        var result = await _mediator.Send(new GetLoanByIdQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new loan
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLoan([FromBody] CreateLoanCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing loan
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLoan(Guid id, [FromBody] UpdateLoanCommand command)
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
    /// Delete a loan (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLoan(Guid id)
    {
        var result = await _mediator.Send(new DeleteLoanCommand(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
