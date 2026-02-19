using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequests.Commands.FeedbackTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.MiscellaneousTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.OvertimeTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.PermissionTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.PersonalTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.TrainingTypes;
using HrSystem.Application.Features.EmployeeRequests.Commands.VacationTypes;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Application.Features.EmployeeRequests.Queries.FeedbackTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.MiscellaneousTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.OvertimeTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.PermissionTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.PersonalTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.TrainingTypes;
using HrSystem.Application.Features.EmployeeRequests.Queries.VacationTypes;
using HrSystem.Application.Features.EmployeeRequests.RequestTypes.Commands;
using HrSystem.Application.Features.EmployeeRequests.RequestTypes.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

/// <summary>
/// Controller for managing request type master data (VacationType, OvertimeType, TrainingType, etc.)
/// SuperAdmin only operations for CRUD on dropdown options.
/// </summary>
[Authorize(Roles = "SuperAdmin")]
[Route("api/[controller]")]
public class RequestTypesController : APIBaseController
{
    private readonly ISender _mediator;

    public RequestTypesController(ISender mediator)
    {
        _mediator = mediator;
    }

    #region VacationType CRUD
    /// <summary>
    /// Get all vacation types
    /// </summary>
    [HttpGet("vacation")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVacationTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetVacationTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get vacation type by ID
    /// </summary>
    [HttpGet("vacation/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetVacationTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetVacationTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new vacation type
    /// </summary>
    [HttpPost("vacation")]
    public async Task<IActionResult> CreateVacationType([FromBody] CreateVacationTypeDto dto)
    {
        var command = new CreateVacationTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.IsPaid, dto.RequiresManagerApproval, dto.RequireAttachment, dto.IsActive, dto.SortOrder, dto.MaxDaysPerYear);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetVacationTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing vacation type
    /// </summary>
    [HttpPut("vacation/{id:guid}")]
    public async Task<IActionResult> UpdateVacationType(Guid id, [FromBody] UpdateVacationTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateVacationTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.IsPaid, dto.RequiresManagerApproval, dto.RequireAttachment, dto.IsActive, dto.SortOrder, dto.MaxDaysPerYear);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a vacation type
    /// </summary>
    [HttpDelete("vacation/{id:guid}")]
    public async Task<IActionResult> DeleteVacationType(Guid id)
    {
        var result = await _mediator.Send(new DeleteVacationTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region OvertimeType CRUD
    /// <summary>
    /// Get all overtime types
    /// </summary>
    [HttpGet("overtime")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOvertimeTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetOvertimeTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get overtime type by ID
    /// </summary>
    [HttpGet("overtime/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOvertimeTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetOvertimeTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new overtime type
    /// </summary>
    [HttpPost("overtime")]
    public async Task<IActionResult> CreateOvertimeType([FromBody] CreateOvertimeTypeDto dto)
    {
        var command = new CreateOvertimeTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.DefaultMultiplier, dto.RequiresManagerApproval, dto.RequireAttachment, dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetOvertimeTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing overtime type
    /// </summary>
    [HttpPut("overtime/{id:guid}")]
    public async Task<IActionResult> UpdateOvertimeType(Guid id, [FromBody] UpdateOvertimeTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateOvertimeTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.DefaultMultiplier, dto.RequiresManagerApproval, dto.RequireAttachment, dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) an overtime type
    /// </summary>
    [HttpDelete("overtime/{id:guid}")]
    public async Task<IActionResult> DeleteOvertimeType(Guid id)
    {
        var result = await _mediator.Send(new DeleteOvertimeTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region TrainingType CRUD
    /// <summary>
    /// Get all training types
    /// </summary>
    [HttpGet("training")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrainingTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetTrainingTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get training type by ID
    /// </summary>
    [HttpGet("training/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTrainingTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainingTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new training type
    /// </summary>
    [HttpPost("training")]
    public async Task<IActionResult> CreateTrainingType([FromBody] CreateTrainingTypeDto dto)
    {
        var command = new CreateTrainingTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetTrainingTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing training type
    /// </summary>
    [HttpPut("training/{id:guid}")]
    public async Task<IActionResult> UpdateTrainingType(Guid id, [FromBody] UpdateTrainingTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateTrainingTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a training type
    /// </summary>
    [HttpDelete("training/{id:guid}")]
    public async Task<IActionResult> DeleteTrainingType(Guid id)
    {
        var result = await _mediator.Send(new DeleteTrainingTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region MiscellaneousType CRUD
    /// <summary>
    /// Get all miscellaneous types
    /// </summary>
    [HttpGet("miscellaneous")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMiscellaneousTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetMiscellaneousTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get miscellaneous type by ID
    /// </summary>
    [HttpGet("miscellaneous/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMiscellaneousTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetMiscellaneousTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new miscellaneous type
    /// </summary>
    [HttpPost("miscellaneous")]
    public async Task<IActionResult> CreateMiscellaneousType([FromBody] CreateMiscellaneousTypeDto dto)
    {
        var command = new CreateMiscellaneousTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetMiscellaneousTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing miscellaneous type
    /// </summary>
    [HttpPut("miscellaneous/{id:guid}")]
    public async Task<IActionResult> UpdateMiscellaneousType(Guid id, [FromBody] UpdateMiscellaneousTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateMiscellaneousTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a miscellaneous type
    /// </summary>
    [HttpDelete("miscellaneous/{id:guid}")]
    public async Task<IActionResult> DeleteMiscellaneousType(Guid id)
    {
        var result = await _mediator.Send(new DeleteMiscellaneousTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region PersonalType CRUD
    /// <summary>
    /// Get all personal types
    /// </summary>
    [HttpGet("personal")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPersonalTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetPersonalTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get personal type by ID
    /// </summary>
    [HttpGet("personal/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPersonalTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetPersonalTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new personal type
    /// </summary>
    [HttpPost("personal")]
    public async Task<IActionResult> CreatePersonalType([FromBody] CreatePersonalTypeDto dto)
    {
        var command = new CreatePersonalTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetPersonalTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing personal type
    /// </summary>
    [HttpPut("personal/{id:guid}")]
    public async Task<IActionResult> UpdatePersonalType(Guid id, [FromBody] UpdatePersonalTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdatePersonalTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a personal type
    /// </summary>
    [HttpDelete("personal/{id:guid}")]
    public async Task<IActionResult> DeletePersonalType(Guid id)
    {
        var result = await _mediator.Send(new DeletePersonalTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region FeedbackType CRUD
    /// <summary>
    /// Get all feedback types
    /// </summary>
    [HttpGet("feedback")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeedbackTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetFeedbackTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get feedback type by ID
    /// </summary>
    [HttpGet("feedback/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeedbackTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetFeedbackTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new feedback type
    /// </summary>
    [HttpPost("feedback")]
    public async Task<IActionResult> CreateFeedbackType([FromBody] CreateFeedbackTypeDto dto)
    {
        var command = new CreateFeedbackTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.IsAnonymousAllowed, dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetFeedbackTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing feedback type
    /// </summary>
    [HttpPut("feedback/{id:guid}")]
    public async Task<IActionResult> UpdateFeedbackType(Guid id, [FromBody] UpdateFeedbackTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateFeedbackTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.IsAnonymousAllowed, dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a feedback type
    /// </summary>
    [HttpDelete("feedback/{id:guid}")]
    public async Task<IActionResult> DeleteFeedbackType(Guid id)
    {
        var result = await _mediator.Send(new DeleteFeedbackTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region PermissionType CRUD
    /// <summary>
    /// Get all permission types (for leave early, come late, short absence requests)
    /// </summary>
    [HttpGet("permission")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPermissionTypes([FromQuery] bool? isActive, [FromQuery] string? searchTerm)
    {
        var result = await _mediator.Send(new GetPermissionTypesQuery(isActive, searchTerm));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get permission type by ID
    /// </summary>
    [HttpGet("permission/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPermissionTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetPermissionTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new permission type
    /// </summary>
    [HttpPost("permission")]
    public async Task<IActionResult> CreatePermissionType([FromBody] CreatePermissionTypeDto dto)
    {
        var command = new CreatePermissionTypeCommand(
            dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(
            response => CreatedAtAction(nameof(GetPermissionTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing permission type
    /// </summary>
    [HttpPut("permission/{id:guid}")]
    public async Task<IActionResult> UpdatePermissionType(Guid id, [FromBody] UpdatePermissionTypeDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdatePermissionTypeCommand(
            dto.Id, dto.NameAr, dto.NameEn, dto.Description,
            dto.RequiresManagerApproval, dto.RequireAttachment,
            dto.IsActive, dto.SortOrder);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a permission type
    /// </summary>
    [HttpDelete("permission/{id:guid}")]
    public async Task<IActionResult> DeletePermissionType(Guid id)
    {
        var result = await _mediator.Send(new DeletePermissionTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion

    #region Master RequestType CRUD
    /// <summary>
    /// Get all master request types (categories like Vacation, OverTime, Training, etc.)
    /// </summary>
    [HttpGet("master")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRequestTypes([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetRequestTypesQuery(includeInactive));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get master request type by ID
    /// </summary>
    [HttpGet("master/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRequestTypeById(Guid id)
    {
        var result = await _mediator.Send(new GetRequestTypeByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create a new master request type
    /// </summary>
    [HttpPost("master")]
    public async Task<IActionResult> CreateRequestType([FromBody] CreateRequestTypeMasterDto dto)
    {
        var result = await _mediator.Send(new CreateRequestTypeCommand(dto));
        return result.Match(
            response => CreatedAtAction(nameof(GetRequestTypeById), new { id = response.Data!.Id }, response),
            Problem);
    }

    /// <summary>
    /// Update an existing master request type
    /// </summary>
    [HttpPut("master/{id:guid}")]
    public async Task<IActionResult> UpdateRequestType(Guid id, [FromBody] UpdateRequestTypeMasterDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var result = await _mediator.Send(new UpdateRequestTypeCommand(dto));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete (soft) a master request type
    /// </summary>
    [HttpDelete("master/{id:guid}")]
    public async Task<IActionResult> DeleteRequestType(Guid id)
    {
        var result = await _mediator.Send(new DeleteRequestTypeCommand(id));
        return result.Match(Ok, Problem);
    }
    #endregion
}
