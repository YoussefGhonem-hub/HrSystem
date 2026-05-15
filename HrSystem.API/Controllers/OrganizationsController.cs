using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationFull;
using HrSystem.Application.Features.Organizations.Commands.CreateOrganizationWithAdmin;
using HrSystem.Application.Features.Organizations.Commands.CreateSubscriptionPlan;
using HrSystem.Application.Features.Organizations.Commands.CollectOrganizationInvoice;
using HrSystem.Application.Features.Organizations.Commands.UpdateBranchHolidays;
using HrSystem.Application.Features.Organizations.Commands.UpdateBranchWorkSchedule;
using HrSystem.Application.Features.Organizations.Commands.GenerateMonthlyOrganizationInvoices;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationBranches;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationCompanyInfo;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationStructure;
using HrSystem.Application.Features.Organizations.Commands.UpdateOrganizationSubscription;
using HrSystem.Application.Features.Organizations.Commands.UpdateSubscriptionPlan;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationDetails;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationAdminDashboard;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationFullDetails;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationTrialStatus;
using HrSystem.Application.Features.Organizations.Queries.GetOrganizationsList;
using HrSystem.Application.Features.Organizations.Queries.GetInvoiceCollections;
using HrSystem.Application.Features.Organizations.Queries.GetSuperAdminInvoices;
using HrSystem.Application.Features.Organizations.Queries.GetSuperAdminBillingDashboard;
using HrSystem.Application.Features.Organizations.Queries.GetSubscriptionPlans;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
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
    /// Get active subscription plans. Only SuperAdmin.
    /// </summary>
    [HttpGet("subscription-plans")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GetSubscriptionPlans([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery(includeInactive));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new subscription plan. Only SuperAdmin.
    /// </summary>
    [HttpPost("subscription-plans")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] UpsertSubscriptionPlanRequest request)
    {
        var command = new CreateSubscriptionPlanCommand(
            Code: request.Code,
            NameEn: request.NameEn,
            NameAr: request.NameAr,
            DescriptionEn: request.DescriptionEn,
            DescriptionAr: request.DescriptionAr,
            MonthlyPrice: request.MonthlyPrice,
            AnnualPrice: request.AnnualPrice,
            Currency: request.Currency,
            MaxEmployees: request.MaxEmployees,
            MaxStorageGB: request.MaxStorageGB,
            MaxDepartments: request.MaxDepartments,
            AllowBiometricIntegration: request.AllowBiometricIntegration,
            AllowPayrollModule: request.AllowPayrollModule,
            AllowPerformanceModule: request.AllowPerformanceModule,
            AllowRecruitmentModule: request.AllowRecruitmentModule,
            AllowCustomReports: request.AllowCustomReports,
            AllowAPIAccess: request.AllowAPIAccess,
            TrialDays: request.TrialDays,
            IsActive: request.IsActive,
            DisplayOrder: request.DisplayOrder);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing subscription plan. Only SuperAdmin.
    /// </summary>
    [HttpPut("subscription-plans/{id:guid}")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, [FromBody] UpsertSubscriptionPlanRequest request)
    {
        var command = new UpdateSubscriptionPlanCommand(
            SubscriptionPlanId: id,
            Code: request.Code,
            NameEn: request.NameEn,
            NameAr: request.NameAr,
            DescriptionEn: request.DescriptionEn,
            DescriptionAr: request.DescriptionAr,
            MonthlyPrice: request.MonthlyPrice,
            AnnualPrice: request.AnnualPrice,
            Currency: request.Currency,
            MaxEmployees: request.MaxEmployees,
            MaxStorageGB: request.MaxStorageGB,
            MaxDepartments: request.MaxDepartments,
            AllowBiometricIntegration: request.AllowBiometricIntegration,
            AllowPayrollModule: request.AllowPayrollModule,
            AllowPerformanceModule: request.AllowPerformanceModule,
            AllowRecruitmentModule: request.AllowRecruitmentModule,
            AllowCustomReports: request.AllowCustomReports,
            AllowAPIAccess: request.AllowAPIAccess,
            TrialDays: request.TrialDays,
            IsActive: request.IsActive,
            DisplayOrder: request.DisplayOrder);

        var result = await _mediator.Send(command);

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
    /// Public endpoint to check if organization trial period is finished.
    /// Use this to decide whether to block app access after trial end.
    /// </summary>
    [HttpGet("{id:guid}/trial-status")]
    [AllowAnonymous]
    public async Task<IActionResult> GetOrganizationTrialStatus(Guid id)
    {
        var result = await _mediator.Send(new GetOrganizationTrialStatusQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get organization-admin dashboard data for the current organization.
    /// </summary>
    [HttpGet("dashboard/org-admin")]
    public async Task<IActionResult> GetOrganizationAdminDashboard(
        [FromQuery] DateTime? date = null,
        [FromQuery] int employeePageNumber = 1,
        [FromQuery] int employeePageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int recentAttendanceCount = 50,
        [FromQuery] int recentLeaveHistoryCount = 20,
        [FromQuery] int recentLeaveRequestsCount = 20)
    {
        var normalizedRoles = CurrentUser.Roles
            .Select(RoleNames.Normalize)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();

        var canAccessDashboard = normalizedRoles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase))
            || normalizedRoles.Any(r => string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase))
            || normalizedRoles.Any(r => string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase))
            || normalizedRoles.Any(r => string.Equals(r, RoleNames.DepartmentManager, StringComparison.OrdinalIgnoreCase));

        if (!canAccessDashboard)
        {
            return Forbid();
        }

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
    /// Get super-admin billing dashboard KPI cards.
    /// </summary>
    [HttpGet("dashboard/super-admin-billing")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GetSuperAdminBillingDashboard()
    {
        var result = await _mediator.Send(new GetSuperAdminBillingDashboardQuery());

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

    #region Subscription Tab

    /// <summary>
    /// Update organization subscription and validate monthly limits against active employees.
    /// </summary>
    [HttpPut("{id:guid}/subscription")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> UpdateSubscription(Guid id, [FromBody] UpdateSubscriptionRequest request)
    {
        var command = new UpdateOrganizationSubscriptionCommand(
            OrganizationId: id,
            SubscriptionPlanId: request.SubscriptionPlanId,
            SubscriptionStartDate: request.SubscriptionStartDate,
            SubscriptionEndDate: request.SubscriptionEndDate,
            BillingCycle: request.BillingCycle);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Generate monthly invoices for all active organizations with active subscription plans.
    /// Generates one invoice per organization for the selected month and skips already-generated invoices.
    /// </summary>
    [HttpPost("invoices/generate-monthly")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GenerateMonthlyInvoices([FromBody] GenerateMonthlyInvoicesRequest? request)
    {
        var command = new GenerateMonthlyOrganizationInvoicesCommand(
            Year: request?.Year,
            Month: request?.Month,
            DueInDays: request?.DueInDays ?? 15);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// List invoices for collection tracking. Supports filtering by collected/uncollected state.
    /// </summary>
    [HttpGet("invoices/collection")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GetInvoicesForCollection([FromQuery] bool? isCollected = null)
    {
        var result = await _mediator.Send(new GetInvoiceCollectionsQuery(isCollected));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// List all invoices for SuperAdmin invoice management screen with line-item details.
    /// </summary>
    [HttpGet("invoices/super-admin")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> GetSuperAdminInvoices()
    {
        var result = await _mediator.Send(new GetSuperAdminInvoicesQuery());

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Mark a specific invoice as collected (paid).
    /// </summary>
    [HttpPut("invoices/{invoiceId:guid}/collect")]
    [Authorize(Roles = RoleNames.SuperAdmin)]
    public async Task<IActionResult> CollectInvoice(Guid invoiceId, [FromBody] CollectInvoiceRequest? request)
    {
        var command = new CollectOrganizationInvoiceCommand(
            InvoiceId: invoiceId,
            PaymentMethod: request?.PaymentMethod,
            PaymentReference: request?.PaymentReference,
            PaidDate: request?.PaidDate,
            Notes: request?.Notes);

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    #endregion

    private IActionResult? EnsureOrganizationAccess(Guid requestedOrganizationId)
    {
        var normalizedRoles = CurrentUser.Roles
            .Select(RoleNames.Normalize)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToArray();

        var isSuperAdmin = normalizedRoles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
        if (isSuperAdmin)
        {
            return null;
        }

        var isAllowedScopedRole =
            normalizedRoles.Any(r => string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase)) ||
            normalizedRoles.Any(r => string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase));

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

public record UpdateSubscriptionRequest(
    Guid SubscriptionPlanId,
    DateTime? SubscriptionStartDate,
    DateTime? SubscriptionEndDate,
    string BillingCycle
);

public record UpsertSubscriptionPlanRequest(
    string Code,
    string NameEn,
    string NameAr,
    string? DescriptionEn,
    string? DescriptionAr,
    decimal MonthlyPrice,
    decimal AnnualPrice,
    string Currency,
    int MaxEmployees,
    int MaxStorageGB,
    int MaxDepartments,
    bool AllowBiometricIntegration,
    bool AllowPayrollModule,
    bool AllowPerformanceModule,
    bool AllowRecruitmentModule,
    bool AllowCustomReports,
    bool AllowAPIAccess,
    int TrialDays,
    bool IsActive,
    int DisplayOrder
);

public record GenerateMonthlyInvoicesRequest(
    int? Year,
    int? Month,
    int? DueInDays
);

public record CollectInvoiceRequest(
    string? PaymentMethod,
    string? PaymentReference,
    DateTime? PaidDate,
    string? Notes
);

#endregion
