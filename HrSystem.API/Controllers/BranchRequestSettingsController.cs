using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.EmployeeRequests.Commands.BranchSettings;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Application.Features.EmployeeRequests.Queries.BranchSettings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

/// <summary>
/// Controller for managing branch-level request settings.
/// Controls which request types are available for employees in each branch.
/// </summary>
[Authorize(Roles = "SuperAdmin,OrganizationAdmin,HRManager,DepartmentManager")]
[Route("api/[controller]")]
public class BranchRequestSettingsController : APIBaseController
{
    private readonly ISender _mediator;

    public BranchRequestSettingsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all branch request settings with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? requestTypeId)
    {
        var result = await _mediator.Send(new GetBranchRequestSettingsQuery(branchId, requestTypeId));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get branch request setting by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetBranchRequestSettingByIdQuery(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get branches summary showing which ones need settings configured
    /// </summary>
    [HttpGet("branches-summary")]
    public async Task<IActionResult> GetBranchesSummary()
    {
        var result = await _mediator.Send(new GetBranchesWithoutSettingsQuery());
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Create multiple branch request settings
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] List<CreateBranchRequestSettingDto> dtos)
    {
        var command = new CreateBranchRequestSettingsCommand(dtos);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Update a branch request setting
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBranchRequestSettingDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");

        var command = new UpdateBranchRequestSettingCommand(
            dto.Id,
            dto.IsVisibleToEmployees,
            dto.AllowEmployeesToSubmit,
            dto.RequireAttachment,
            dto.MaxOpenRequests,
            dto.CustomInstructions);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Delete a branch request setting (hard delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteBranchRequestSettingCommand(id));
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Initialize all request type settings for a branch.
    /// Creates settings for all 6 request types if they don't exist.
    /// </summary>
    [HttpPost("branches/{branchId:guid}/initialize")]
    public async Task<IActionResult> InitializeBranchSettings(Guid branchId, [FromBody] InitializeBranchSettingsDto? dto)
    {
        var command = new InitializeBranchSettingsCommand(
            branchId,
            dto?.EnableAllRequestTypes ?? true);

        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get all settings for a specific branch
    /// </summary>
    [HttpGet("branches/{branchId:guid}")]
    public async Task<IActionResult> GetByBranch(Guid branchId)
    {
        var result = await _mediator.Send(new GetBranchRequestSettingsQuery(branchId, null));
        return result.Match(Ok, Problem);
    }
}
