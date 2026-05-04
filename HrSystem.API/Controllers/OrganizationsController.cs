using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationFull;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationWithAdmin;
using HrSystem.Application.Features.Organizations.Commands.UpdateBranchHolidays;
using HrSystem.Application.Features.Organizations.Commands.UpdateBranchWorkSchedule;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationBranches;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationCompanyInfo;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationStructure;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationDetails;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationAdminDashboard;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationFullDetails;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationsList;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.HRManager + "," + RoleNames.OrganizationAdmin)]
public class OrganizationsController : APIBaseController
{
    private readonly ISender _mediator;

    public OrganizationsController(ISender mediator)
    {
        _mediator = mediator;
    }

    #region Organization CRUD

    /// <summary>
    /// Create a new organization with branches and an OrganizationAdmin user (basic)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationWithAdminCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetOrganizationById), new { id = response.Data!.OrganizationId }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a complete organization with all components: branches, departments, job titles, schedules, holidays, and admin user
    /// </summary>
    [HttpPost("full")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> CreateOrganizationFull([FromBody] CreateOrganizationFullCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => CreatedAtAction(nameof(GetOrganizationById), new { id = response.Data!.OrganizationId }, response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get all organizations (paginated). Only SuperAdmin.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GetOrganizations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetOrganizationsListQuery(pageNumber, pageSize, searchTerm));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get organization details by id (basic).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrganizationById(Guid id)
    {
        var accessCheck = EnsureOrganizationAccess(id);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var result = await _mediator.Send(new GetOrganizationDetailsQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get organization-admin dashboard data for the current organization.
    /// </summary>
    [HttpGet("dashboard/org-admin")]
    [Authorize(Roles = RoleNames.SuperAdmin + "," + RoleNames.OrganizationAdmin)]
    public async Task<IActionResult> GetOrganizationAdminDashboard(
        [FromQuery] DateTime? date = null,
        [FromQuery] int employeePageNumber = 1,
        [FromQuery] int employeePageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int recentAttendanceCount = 50,
        [FromQuery] int recentLeaveHistoryCount = 20,
        [FromQuery] int recentLeaveRequestsCount = 20)
    {
        var query = new GetOrganizationAdminDashboardQuery(
            date,
            employeePageNumber,
            employeePageSize,
            searchTerm,
            recentAttendanceCount,
            recentLeaveHistoryCount,
            recentLeaveRequestsCount);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get full organization details including all tabs data: company info, branches, structure, schedules, holidays
    /// </summary>
    [HttpGet("{id:guid}/full")]
    public async Task<IActionResult> GetOrganizationFullDetails(Guid id)
    {
        var accessCheck = EnsureOrganizationAccess(id);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var result = await _mediator.Send(new GetOrganizationFullDetailsQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    #region Company Info Tab

    /// <summary>
    /// Update organization company profile information (Company Info Tab)
    /// </summary>
    [HttpPut("{id:guid}/company-info")]
    public async Task<IActionResult> UpdateCompanyInfo(Guid id, [FromBody] UpdateCompanyInfoRequest request)
    {
        var accessCheck = EnsureOrganizationAccess(id);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var command = new UpdateOrganizationCompanyInfoCommand(
            OrganizationId: id,
            NameAr: request.NameAr,
            NameEn: request.NameEn,
            Industry: request.Industry,
            LogoUrl: request.LogoUrl,
            CommercialRegistrationNumber: request.CommercialRegistrationNumber,
            TaxRegistrationNumber: request.TaxRegistrationNumber,
            LegalEntityType: request.LegalEntityType,
            Email: request.Email,
            PhoneNumber: request.PhoneNumber,
            SecondaryPhoneNumber: request.SecondaryPhoneNumber,
            Website: request.Website,
            AddressAr: request.AddressAr,
            AddressEn: request.AddressEn,
            City: request.City,
            Country: request.Country,
            PostalCode: request.PostalCode,
            TimeZone: request.TimeZone,
            Currency: request.Currency,
            WeekStartDay: request.WeekStartDay,
            DefaultLanguage: request.DefaultLanguage
        );

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    #region Branches Tab

    /// <summary>
    /// Update organization branches (Branches Tab) - supports add, update, delete operations
    /// </summary>
    [HttpPut("{id:guid}/branches")]
    public async Task<IActionResult> UpdateBranches(Guid id, [FromBody] UpdateBranchesRequest request)
    {
        var accessCheck = EnsureOrganizationAccess(id);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var command = new UpdateOrganizationBranchesCommand(id, request.Branches);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    #region Structure Tab (Departments & Job Titles)

    /// <summary>
    /// Update organization structure - departments and job titles (Structure Tab)
    /// </summary>
    [HttpPut("{id:guid}/structure")]
    public async Task<IActionResult> UpdateStructure(Guid id, [FromBody] UpdateStructureRequest request)
    {
        var accessCheck = EnsureOrganizationAccess(id);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var command = new UpdateOrganizationStructureCommand(id, request.Departments, request.JobTitles);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    #region Work Schedule Tab

    /// <summary>
    /// Update work schedules for a specific branch (Work Schedule Tab)
    /// </summary>
    [HttpPut("{organizationId:guid}/branches/{branchId:guid}/work-schedules")]
    public async Task<IActionResult> UpdateBranchWorkSchedules(
        Guid organizationId,
        Guid branchId,
        [FromBody] UpdateWorkSchedulesRequest request)
    {
        var accessCheck = EnsureOrganizationAccess(organizationId);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var command = new UpdateBranchWorkScheduleCommand(organizationId, branchId, request.Schedules);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    #region Holidays Tab

    /// <summary>
    /// Update holidays for a specific branch (Holidays Tab)
    /// </summary>
    [HttpPut("{organizationId:guid}/branches/{branchId:guid}/holidays")]
    public async Task<IActionResult> UpdateBranchHolidays(
        Guid organizationId,
        Guid branchId,
        [FromBody] UpdateHolidaysRequest request)
    {
        var accessCheck = EnsureOrganizationAccess(organizationId);
        if (accessCheck is not null)
        {
            return accessCheck;
        }

        var command = new UpdateBranchHolidaysCommand(organizationId, branchId, request.Holidays);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    private IActionResult? EnsureOrganizationAccess(Guid requestedOrganizationId)
    {
        if (User.IsInRole(RoleNames.SuperAdmin))
        {
            return null;
        }

        var isAllowedScopedRole =
            User.IsInRole(RoleNames.HRManager) ||
            User.IsInRole(RoleNames.OrganizationAdmin);

        if (!isAllowedScopedRole)
        {
            return Forbid();
        }

        var currentOrganizationId = CurrentUser.OrganizationId;
        if (!currentOrganizationId.HasValue || currentOrganizationId.Value == Guid.Empty)
        {
            return Forbid();
        }

        if (currentOrganizationId.Value != requestedOrganizationId)
        {
            return Forbid();
        }

        return null;
    }
}

#region Request DTOs

public record UpdateCompanyInfoRequest(
    string? NameAr,
    string? NameEn,
    string? Industry,
    string? LogoUrl,
    string? CommercialRegistrationNumber,
    string? TaxRegistrationNumber,
    string? LegalEntityType,
    string? Email,
    string? PhoneNumber,
    string? SecondaryPhoneNumber,
    string? Website,
    string? AddressAr,
    string? AddressEn,
    string? City,
    string? Country,
    string? PostalCode,
    string? TimeZone,
    string? Currency,
    string? WeekStartDay,
    string? DefaultLanguage
);

public record UpdateBranchesRequest(List<BranchUpdateInput> Branches);

public record UpdateStructureRequest(
    List<DepartmentUpdateInput>? Departments,
    List<JobTitleUpdateInput>? JobTitles
);

public record UpdateWorkSchedulesRequest(List<WorkScheduleUpdateInput> Schedules);

public record UpdateHolidaysRequest(List<HolidayUpdateInput> Holidays);

#endregion
