using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Branches.Commands.CreateBranch;
using HrSystem.Application.Features.Branches.Commands.DeleteBranch;
using HrSystem.Application.Features.Branches.Commands.UpdateBranch;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Application.Features.Branches.Queries.GetBranchesList;
using HrSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class BranchesController : APIBaseController
{
    private readonly ISender _mediator;

    public BranchesController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of branches with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBranches(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? countryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetBranchesListQuery(
            pageNumber,
            pageSize,
            searchTerm,
            countryId,
            isActive,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get branch by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBranchById(Guid id)
    {
        var query = new GetBranchByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new branch
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetBranchById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing branch
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] UpdateBranchCommand command)
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
    /// Delete a branch (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        var command = new DeleteBranchCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
