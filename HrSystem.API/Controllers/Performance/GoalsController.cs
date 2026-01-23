using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Performance.Goals.Commands.CreateGoal;
using HrSystem.Application.Features.Performance.Goals.Commands.DeleteGoal;
using HrSystem.Application.Features.Performance.Goals.Commands.UpdateGoal;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers.Performance;

[Route("api/[controller]")]
[Authorize]
public class GoalsController : APIBaseController
{
    private readonly ISender _mediator;

    public GoalsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of goals with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGoals(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] DateTime? startDateFrom = null,
        [FromQuery] DateTime? startDateTo = null,
        [FromQuery] DateTime? targetDateFrom = null,
        [FromQuery] DateTime? targetDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetGoalsListQuery(
            employeeId,
            status,
            priority,
            startDateFrom,
            startDateTo,
            targetDateFrom,
            targetDateTo,
            sortBy,
            isDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get goal by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGoalById(Guid id)
    {
        var query = new GetGoalByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new goal
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGoal([FromBody] CreateGoalCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetGoalById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing goal
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGoal(Guid id, [FromBody] UpdateGoalCommand command)
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
    /// Delete a goal (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        var command = new DeleteGoalCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
