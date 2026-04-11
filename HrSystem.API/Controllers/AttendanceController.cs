using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Attendance.Commands.CheckInOut;
using HrSystem.Application.Features.Attendance.Commands.CreateAttendance;
using HrSystem.Application.Features.Attendance.Commands.CreateCheckInPoint;
using HrSystem.Application.Features.Attendance.Commands.DeleteAttendance;
using HrSystem.Application.Features.Attendance.Commands.DeleteCheckInPoint;
using HrSystem.Application.Features.Attendance.Commands.EnrollEmployeeBiometric;
using HrSystem.Application.Features.Attendance.Commands.ImportAttendanceExcel;
using HrSystem.Application.Features.Attendance.Commands.QuickCheckInOut;
using HrSystem.Application.Features.Attendance.Commands.UpdateAttendance;
using HrSystem.Application.Features.Attendance.Commands.UpdateCheckInPoint;
using HrSystem.Application.Features.Attendance.Commands.UpsertBranchAttendanceSetting;
using HrSystem.Application.Features.Attendance.Commands.VerifyBiometricAttendance;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;
using HrSystem.Application.Features.Attendance.Queries.GetAttendanceDashboard;
using HrSystem.Application.Features.Attendance.Queries.GetBranchAttendanceSetting;
using HrSystem.Application.Features.Attendance.Queries.GetBranchCheckInPoints;
using HrSystem.Domain.Enums;
using HrSystem.Shared.Constants;
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
    public async Task<IActionResult> GetAttendance(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] bool? isLate = null,
        [FromQuery] bool? isOvertime = null,
        [FromQuery] string? searchTerm = null,
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
            searchTerm,
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
    /// Alias: Attendance history with filters and pagination
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetAttendanceHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] bool? isLate = null,
        [FromQuery] bool? isOvertime = null,
        [FromQuery] string? searchTerm = null,
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
            searchTerm,
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
    /// Get attendance dashboard statistics for a date (defaults to today)
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetAttendanceDashboard([FromQuery] DateTime? date = null)
    {
        var result = await _mediator.Send(new GetAttendanceDashboardQuery(date));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get attendance record by ID
    /// </summary>
    [HttpGet("{id:guid}")]
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
    public async Task<IActionResult> DeleteAttendance(Guid id)
    {
        var command = new DeleteAttendanceCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Enroll biometric template for an employee (HR or owner)
    /// </summary>
    [HttpPost("biometrics/enroll")]
    public async Task<IActionResult> EnrollBiometric([FromBody] EnrollEmployeeBiometricCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Verify biometric and record attendance (check-in/out)
    /// </summary>
    [HttpPost("biometrics/verify")]
    public async Task<IActionResult> VerifyBiometric([FromBody] VerifyBiometricAttendanceCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════
    //  CHECK-IN / CHECK-OUT (Multi-method: FaceId, Location, Manual)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Unified check-in/check-out endpoint supporting multiple attendance methods.
    /// Validates against branch settings and check-in points (geofencing).
    /// </summary>
    [HttpPost("check")]
    public async Task<IActionResult> CheckInOut([FromBody] CheckInOutCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Quick check-in/check-out for HR Manager and HR Specialist.
    /// Request only needs EmployeeId, PunchType, and EventDateTime.
    /// </summary>
    [HttpPost("quick-check")]
    [Authorize(Roles = RoleNames.HRManager + "," + RoleNames.HRSpecialist + "," + RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> QuickCheckInOut([FromBody] QuickCheckInOutCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════
    //  BRANCH ATTENDANCE SETTINGS
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Get attendance settings for a branch (methods, geofencing, rules).
    /// Returns defaults if no settings have been configured yet.
    /// </summary>
    [HttpGet("branches/{branchId:guid}/settings")]
    public async Task<IActionResult> GetBranchAttendanceSetting(Guid branchId)
    {
        var result = await _mediator.Send(new GetBranchAttendanceSettingQuery(branchId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create or update attendance settings for a branch.
    /// Configures which methods are allowed (FaceId, Location, Excel, Fingerprint, Manual),
    /// geofencing, auto-checkout, and other attendance rules.
    /// </summary>
    [HttpPut("branches/{branchId:guid}/settings")]
    public async Task<IActionResult> UpsertBranchAttendanceSetting(
        Guid branchId, [FromBody] UpsertBranchAttendanceSettingCommand command)
    {
        if (branchId != command.BranchId)
            return BadRequest("Branch ID mismatch.");

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════
    //  BRANCH CHECK-IN POINTS (Geofence locations)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Get all check-in/check-out points for a branch.
    /// </summary>
    [HttpGet("branches/{branchId:guid}/check-in-points")]
    public async Task<IActionResult> GetBranchCheckInPoints(Guid branchId)
    {
        var result = await _mediator.Send(new GetBranchCheckInPointsQuery(branchId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Add a check-in/check-out point (GPS location) to a branch.
    /// </summary>
    [HttpPost("branches/{branchId:guid}/check-in-points")]
    public async Task<IActionResult> CreateCheckInPoint(
        Guid branchId, [FromBody] CreateCheckInPointCommand command)
    {
        if (branchId != command.BranchId)
            return BadRequest("Branch ID mismatch.");

        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetBranchCheckInPoints), new { branchId }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update a check-in/check-out point.
    /// </summary>
    [HttpPut("check-in-points/{id:guid}")]
    public async Task<IActionResult> UpdateCheckInPoint(Guid id, [FromBody] UpdateCheckInPointCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Delete a check-in/check-out point (soft delete).
    /// </summary>
    [HttpDelete("check-in-points/{id:guid}")]
    public async Task<IActionResult> DeleteCheckInPoint(Guid id)
    {
        var result = await _mediator.Send(new DeleteCheckInPointCommand(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    // ══════════════════════════════════════════════════════════
    //  EXCEL IMPORT (Fingerprint device export)
    // ══════════════════════════════════════════════════════════

    /// <summary>
    /// Import attendance records from an Excel/CSV file (fingerprint device export).
    /// Expected columns: EmployeeCode, Date, CheckInTime, CheckOutTime.
    /// Supports .xlsx, .xls, and .csv formats.
    /// </summary>
    [HttpPost("branches/{branchId:guid}/import")]
    public async Task<IActionResult> ImportAttendanceExcel(
        Guid branchId,
        IFormFile file,
        [FromQuery] bool skipDuplicates = true)
    {
        var command = new ImportAttendanceExcelCommand(branchId, file, skipDuplicates);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
