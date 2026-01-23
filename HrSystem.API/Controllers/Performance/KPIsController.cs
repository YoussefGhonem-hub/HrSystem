using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Performance.KPIs.Commands.CreateKPI;
using HrSystem.Application.Features.Performance.KPIs.Commands.DeleteKPI;
using HrSystem.Application.Features.Performance.KPIs.Commands.UpdateKPI;
using HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIById;
using HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers.Performance;

[Route("api/[controller]")]
[Authorize]
public class KPIsController : APIBaseController
{
    private readonly ISender _mediator;

    public KPIsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of KPIs with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKPIs(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        [FromQuery] Guid? jobTitleId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetKPIsListQuery(
            pageNumber,
            pageSize,
            searchTerm,
            category,
            jobTitleId,
            departmentId,
            null,
            null,
            sortBy,
            isDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get KPI by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetKPIById(Guid id)
    {
        var query = new GetKPIByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new KPI
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateKPI([FromBody] CreateKPICommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetKPIById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing KPI
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateKPI(Guid id, [FromBody] UpdateKPICommand command)
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
    /// Delete a KPI (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteKPI(Guid id)
    {
        var command = new DeleteKPICommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
