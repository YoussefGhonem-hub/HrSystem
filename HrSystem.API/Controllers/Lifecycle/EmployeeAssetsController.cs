using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.CreateEmployeeAsset;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.DeleteEmployeeAsset;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.UpdateEmployeeAsset;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetMyAssetsList;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers.Lifecycle;

[Route("api/[controller]")]
[Authorize]
public class EmployeeAssetsController : APIBaseController
{
    private readonly ISender _mediator;

    public EmployeeAssetsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of employee assets with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEmployeeAssets(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] string? assetType = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? assignedDateFrom = null,
        [FromQuery] DateTime? assignedDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetEmployeeAssetsListQuery(
            employeeId,
            assetType,
            status == "Returned" ? true : status == "Assigned" ? false : null,
            assignedDateFrom,
            assignedDateTo,
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
    /// Get assets for the currently logged-in employee
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyAssets(
        [FromQuery] string? assetType = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? assignedDateFrom = null,
        [FromQuery] DateTime? assignedDateTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool isDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetMyAssetsListQuery(
            assetType,
            status == "Returned" ? true : status == "Assigned" ? false : null,
            assignedDateFrom,
            assignedDateTo,
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
    /// Get employee asset by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeAssetById(Guid id)
    {
        var query = new GetEmployeeAssetByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new employee asset assignment
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployeeAsset([FromBody] CreateEmployeeAssetCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetEmployeeAssetById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing employee asset assignment
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmployeeAsset(Guid id, [FromBody] UpdateEmployeeAssetCommand command)
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
    /// Delete an employee asset assignment (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployeeAsset(Guid id)
    {
        var command = new DeleteEmployeeAssetCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
