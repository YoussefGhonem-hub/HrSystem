using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.CreateOnboardingTask;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.DeleteOnboardingTask;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.UpdateOnboardingTask;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTasksList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers.Lifecycle;

[Route("api/[controller]")]
[Authorize]
public class OnboardingTasksController : APIBaseController
{
    private readonly ISender _mediator;

    public OnboardingTasksController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of onboarding tasks with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOnboardingTasks(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] bool? isCompleted = null,
        [FromQuery] string? category = null,
        [FromQuery] DateTime? dueDateFrom = null,
        [FromQuery] DateTime? dueDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetOnboardingTasksListQuery(
            employeeId,
            isCompleted,
            category,
            dueDateFrom,
            dueDateTo,
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
    /// Get onboarding task by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOnboardingTaskById(Guid id)
    {
        var query = new GetOnboardingTaskByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new onboarding task
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOnboardingTask([FromBody] CreateOnboardingTaskCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetOnboardingTaskById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing onboarding task
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateOnboardingTask(Guid id, [FromBody] UpdateOnboardingTaskCommand command)
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
    /// Delete an onboarding task (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteOnboardingTask(Guid id)
    {
        var command = new DeleteOnboardingTaskCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
