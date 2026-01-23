using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Attendance.Commands.CreateAttendance;
using HrSystem.Application.Features.Attendance.Commands.DeleteAttendance;
using HrSystem.Application.Features.Attendance.Commands.UpdateAttendance;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;
using HrSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AttendanceController : APIBaseController
{
    private readonly ISender _mediator;

    public AttendanceController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get a paginated list of attendance records with filters and sorting
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] bool? isLate = null,
        [FromQuery] bool? isOvertime = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetAttendancesListQuery(
            employeeId,
            startDate,
            endDate,
            statusId,
            isLate,
            isOvertime,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get attendance record by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttendanceById(Guid id)
    {
        var query = new GetAttendanceByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new attendance record
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAttendance([FromBody] CreateAttendanceCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetAttendanceById), new { id = response.Data!.Id }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing attendance record
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAttendance(Guid id, [FromBody] UpdateAttendanceCommand command)
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
    /// Delete an attendance record (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAttendance(Guid id)
    {
        var command = new DeleteAttendanceCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
