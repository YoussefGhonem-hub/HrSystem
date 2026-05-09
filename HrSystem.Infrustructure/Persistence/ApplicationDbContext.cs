using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Leave;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Payroll;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Infrustructure.Extensions;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HrSystem.Infrustructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // â”€â”€ Scope filter properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    // EF Core evaluates these per-query when referenced inside global query filters.
    // SuperAdmin  â†’ no filter at all
    // OrgAdmin    â†’ filter by TenantId only
    // Others      â†’ filter by BranchId only
    public Guid CurrentTenantId => CurrentUser.OrganizationId ?? Guid.Empty;
    public Guid CurrentBranchId => CurrentUser.BranchId ?? Guid.Empty;
    public Guid CurrentEmployeeId => CurrentUser.EmployeeId ?? Guid.Empty;
    public Guid CurrentUserId => CurrentUser.Id ?? Guid.Empty;

    public bool FilterBypassEnabled => CurrentUser.BypassScopeFilters
        || (CurrentUser.IsSuperAdmin && !CurrentUser.SelectedOrganizationId.HasValue);

    public bool IsOrgAdminScope => CurrentUser.Roles.Any(r =>
        string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase));

    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserBranchRole> UserBranchRoles => Set<UserBranchRole>();

    // Employee Management
    public DbSet<HrSystem.Domain.Entities.Employee.Employee> Employees => Set<HrSystem.Domain.Entities.Employee.Employee>();
    public DbSet<HrSystem.Domain.Entities.Employee.Department> Departments => Set<HrSystem.Domain.Entities.Employee.Department>();
    public DbSet<HrSystem.Domain.Entities.Employee.JobTitle> JobTitles => Set<HrSystem.Domain.Entities.Employee.JobTitle>();
    public DbSet<HrSystem.Domain.Entities.Employee.EmployeeDocument> EmployeeDocuments => Set<HrSystem.Domain.Entities.Employee.EmployeeDocument>();

    // Payroll
    public DbSet<HrSystem.Domain.Entities.Payroll.Salary> Salaries => Set<HrSystem.Domain.Entities.Payroll.Salary>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryAllowance> SalaryAllowances => Set<HrSystem.Domain.Entities.Payroll.SalaryAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryDeduction> SalaryDeductions => Set<HrSystem.Domain.Entities.Payroll.SalaryDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.Loan> Loans => Set<HrSystem.Domain.Entities.Payroll.Loan>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayrollCycle> PayrollCycles => Set<HrSystem.Domain.Entities.Payroll.PayrollCycle>();
    public DbSet<HrSystem.Domain.Entities.Payroll.Payslip> Payslips => Set<HrSystem.Domain.Entities.Payroll.Payslip>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipAllowance> PayslipAllowances => Set<HrSystem.Domain.Entities.Payroll.PayslipAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipDeduction> PayslipDeductions => Set<HrSystem.Domain.Entities.Payroll.PayslipDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.TaxBracket> TaxBrackets => Set<HrSystem.Domain.Entities.Payroll.TaxBracket>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate> SocialInsuranceRates => Set<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate>();
    public DbSet<HrSystem.Domain.Entities.Payroll.BankExportProfile> BankExportProfiles => Set<HrSystem.Domain.Entities.Payroll.BankExportProfile>();

    // Attendance
    public DbSet<HrSystem.Domain.Entities.Attendance.Attendance> Attendances => Set<HrSystem.Domain.Entities.Attendance.Attendance>();
    public DbSet<HrSystem.Domain.Entities.Attendance.EmployeeBiometric> EmployeeBiometrics => Set<HrSystem.Domain.Entities.Attendance.EmployeeBiometric>();
    public DbSet<HrSystem.Domain.Entities.Attendance.BranchAttendanceSetting> BranchAttendanceSettings => Set<HrSystem.Domain.Entities.Attendance.BranchAttendanceSetting>();
    public DbSet<HrSystem.Domain.Entities.Attendance.BranchCheckInPoint> BranchCheckInPoints => Set<HrSystem.Domain.Entities.Attendance.BranchCheckInPoint>();

    // Lookup Tables
    public DbSet<HrSystem.Domain.Entities.Attendance.AttendanceStatus> AttendanceStatuses => Set<HrSystem.Domain.Entities.Attendance.AttendanceStatus>();
    public DbSet<HrSystem.Domain.Entities.Employee.ContractType> ContractTypes => Set<HrSystem.Domain.Entities.Employee.ContractType>();
    public DbSet<HrSystem.Domain.Entities.Employee.Gender> Genders => Set<HrSystem.Domain.Entities.Employee.Gender>();
    public DbSet<HrSystem.Domain.Entities.Employee.MaritalStatus> MaritalStatuses => Set<HrSystem.Domain.Entities.Employee.MaritalStatus>();
    public DbSet<HrSystem.Domain.Entities.Employee.EmployeeStatus> EmployeeStatuses => Set<HrSystem.Domain.Entities.Employee.EmployeeStatus>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayrollStatus> PayrollStatuses => Set<HrSystem.Domain.Entities.Payroll.PayrollStatus>();
    public DbSet<HrSystem.Domain.Entities.Organization.Country> Countries => Set<HrSystem.Domain.Entities.Organization.Country>();

    // Performance Management
    public DbSet<HrSystem.Domain.Entities.Performance.PerformanceReview> PerformanceReviews => Set<HrSystem.Domain.Entities.Performance.PerformanceReview>();
    public DbSet<HrSystem.Domain.Entities.Performance.ReviewType> ReviewTypes => Set<HrSystem.Domain.Entities.Performance.ReviewType>();
    public DbSet<HrSystem.Domain.Entities.Performance.ReviewStatus> ReviewStatuses => Set<HrSystem.Domain.Entities.Performance.ReviewStatus>();
    public DbSet<HrSystem.Domain.Entities.Performance.KPI> KPIs => Set<HrSystem.Domain.Entities.Performance.KPI>();
    public DbSet<HrSystem.Domain.Entities.Performance.KPIEvaluation> KPIEvaluations => Set<HrSystem.Domain.Entities.Performance.KPIEvaluation>();
    public DbSet<HrSystem.Domain.Entities.Performance.Goal> Goals => Set<HrSystem.Domain.Entities.Performance.Goal>();
    public DbSet<HrSystem.Domain.Entities.Performance.GoalStatus> GoalStatuses => Set<HrSystem.Domain.Entities.Performance.GoalStatus>();
    public DbSet<HrSystem.Domain.Entities.Performance.GoalPriority> GoalPriorities => Set<HrSystem.Domain.Entities.Performance.GoalPriority>();
    public DbSet<HrSystem.Domain.Entities.Performance.GoalMilestone> GoalMilestones => Set<HrSystem.Domain.Entities.Performance.GoalMilestone>();
    public DbSet<HrSystem.Domain.Entities.Performance.Feedback> Feedbacks => Set<HrSystem.Domain.Entities.Performance.Feedback>();

    // Lifecycle Management
    public DbSet<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset> EmployeeAssets => Set<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset>();

    // Organization & Multi-Tenancy
    public DbSet<HrSystem.Domain.Entities.Organization.Organization> Organizations => Set<HrSystem.Domain.Entities.Organization.Organization>();
    public DbSet<HrSystem.Domain.Entities.Organization.Branch> Branches => Set<HrSystem.Domain.Entities.Organization.Branch>();
    public DbSet<HrSystem.Domain.Entities.Organization.BranchWorkSchedule> BranchWorkSchedules => Set<HrSystem.Domain.Entities.Organization.BranchWorkSchedule>();
    public DbSet<HrSystem.Domain.Entities.Organization.BranchHoliday> BranchHolidays => Set<HrSystem.Domain.Entities.Organization.BranchHoliday>();
    public DbSet<HrSystem.Domain.Entities.Organization.SubscriptionPlan> SubscriptionPlans => Set<HrSystem.Domain.Entities.Organization.SubscriptionPlan>();
    public DbSet<HrSystem.Domain.Entities.Organization.OrganizationInvoice> OrganizationInvoices => Set<HrSystem.Domain.Entities.Organization.OrganizationInvoice>();
    public DbSet<HrSystem.Domain.Entities.Organization.InvoiceStatus> InvoiceStatuses => Set<HrSystem.Domain.Entities.Organization.InvoiceStatus>();
    public DbSet<HrSystem.Domain.Entities.Organization.OrganizationInvoiceItem> OrganizationInvoiceItems => Set<HrSystem.Domain.Entities.Organization.OrganizationInvoiceItem>();

    // Employee Self-Service Requests
    public DbSet<EmployeeRequest> EmployeeRequests => Set<EmployeeRequest>();
    public DbSet<BranchRequestSetting> BranchRequestSettings => Set<BranchRequestSetting>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    
    // Request Type Masters
    public DbSet<VacationType> VacationTypes => Set<VacationType>();
    public DbSet<OvertimeType> OvertimeTypes => Set<OvertimeType>();
    public DbSet<TrainingType> TrainingTypes => Set<TrainingType>();
    public DbSet<MiscellaneousType> MiscellaneousTypes => Set<MiscellaneousType>();
    public DbSet<PersonalType> PersonalTypes => Set<PersonalType>();
    public DbSet<FeedbackType> FeedbackTypes => Set<FeedbackType>();
    public DbSet<PermissionType> PermissionTypes => Set<PermissionType>();
    public DbSet<AttendanceCorrectionType> AttendanceCorrectionTypes => Set<AttendanceCorrectionType>();
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
    public DbSet<EmployeeLeaveTransaction> EmployeeLeaveTransactions => Set<EmployeeLeaveTransaction>();
    public DbSet<EmployeeVacationLimit> EmployeeVacationLimits => Set<EmployeeVacationLimit>();
    public DbSet<EmployeePermissionLimit> EmployeePermissionLimits => Set<EmployeePermissionLimit>();
    public DbSet<EmployeeGlobalPermissionLimit> EmployeeGlobalPermissionLimits => Set<EmployeeGlobalPermissionLimit>();
    
    // Request Details
    public DbSet<VacationRequestDetail> VacationRequestDetails => Set<VacationRequestDetail>();
    public DbSet<OvertimeRequestDetail> OvertimeRequestDetails => Set<OvertimeRequestDetail>();
    public DbSet<TrainingRequestDetail> TrainingRequestDetails => Set<TrainingRequestDetail>();
    public DbSet<MiscellaneousRequestDetail> MiscellaneousRequestDetails => Set<MiscellaneousRequestDetail>();
    public DbSet<PersonalRequestDetail> PersonalRequestDetails => Set<PersonalRequestDetail>();
    public DbSet<FeedbackRequestDetail> FeedbackRequestDetails => Set<FeedbackRequestDetail>();
    public DbSet<PermissionRequestDetail> PermissionRequestDetails => Set<PermissionRequestDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.GetOnlyNotDeletedEntities();

        // Apply Multi-Tenancy + Branch Global Query Filters
        ApplyScopedFilters(modelBuilder);
    }

    /// <summary>
    /// Shared lookup entity types that are exempt from the tenant filter.
    /// These are reference data seeded once and shared across all tenants.
    /// Without this exemption, Include() on required FK relationships (INNER JOIN)
    /// would filter out parent rows when the lookup's TenantId doesn't match
    /// the current user's tenant.
    /// </summary>
    private static readonly HashSet<Type> SharedLookupTypes = new()
    {
        typeof(HrSystem.Domain.Entities.Employee.Gender),
        typeof(HrSystem.Domain.Entities.Employee.MaritalStatus),
        typeof(HrSystem.Domain.Entities.Employee.EmployeeStatus),
        typeof(HrSystem.Domain.Entities.Employee.ContractType),
        typeof(AttendanceStatus),
        typeof(PayrollStatus),
        typeof(ReviewType),
        typeof(ReviewStatus),
        typeof(InvoiceStatus),
        typeof(Country),
    };

    /// <summary>
    /// Builds per-entity global query filters that reference DbContext instance properties
    /// so EF Core re-evaluates them on every query (not cached at model-creation time).
    ///
    /// Rules:
    ///   - Shared lookup entities          -> NO tenant/branch filter (shared across tenants)
    ///   - BypassScopeFilters / SuperAdmin -> no scope filter
    ///   - All other roles                -> entity.TenantId == CurrentTenantId
    ///
    /// Branch-level filtering is NOT applied globally because it breaks Include() joins --
    /// navigation entities (Department, JobTitle, Branch, Status) would be filtered out,
    /// causing parent rows to disappear. Use ApplyBranchScope() at the query level instead.
    /// </summary>
    private void ApplyScopedFilters(ModelBuilder modelBuilder)
    {
        // Cache PropertyInfo once -- they don't change.
        var dbContextType = typeof(ApplicationDbContext);
        var bypassProp = dbContextType.GetProperty(nameof(FilterBypassEnabled))!;
        var tenantProp = dbContextType.GetProperty(nameof(CurrentTenantId))!;
        var empIdProp = dbContextType.GetProperty(nameof(CurrentEmployeeId))!;
        var userIdProp = dbContextType.GetProperty(nameof(CurrentUserId))!;
        
        // Expression that represents "this" DbContext instance.
        var dbContextExpr = Expression.Constant(this);

        // Shared sub-expressions (reference DbContext properties -> evaluated per query).
        var bypassExpr = Expression.Property(dbContextExpr, bypassProp);               // bool
        var tenantIdExpr = Expression.Property(dbContextExpr, tenantProp);              // Guid
        var currentEmpIdExpr = Expression.Property(dbContextExpr, empIdProp);           // Guid
        var currentUserIdExpr = Expression.Property(dbContextExpr, userIdProp);         // Guid
        var guidEmptyExpr = Expression.Constant(Guid.Empty);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            // Skip tenant filter for shared lookup entities -- they are reference data
            // shared across all tenants. Applying tenant filter here would break Include()
            // INNER JOINs when the lookup's TenantId doesn't match the querying tenant.
            if (SharedLookupTypes.Contains(entityType.ClrType))
                continue;

            var param = Expression.Parameter(entityType.ClrType, "e");

            // entity.TenantId == this.CurrentTenantId
            var entityTenant = Expression.Property(param, nameof(BaseEntity.TenantId));
            var tenantMatch = Expression.Equal(entityTenant, tenantIdExpr);

            // All entities: bypass OR tenantMatch
            // Branch filtering is applied at query level via ApplyBranchScope(), not globally,
            // to avoid filtering out navigation entities loaded via Include().
            Expression scopePredicate = Expression.OrElse(bypassExpr, tenantMatch);

            // For the Employee entity, allow self-access bypassing tenant scope:
            //   (CurrentEmployeeId != Guid.Empty AND entity.Id == CurrentEmployeeId)
            //   OR (CurrentUserId != Guid.Empty AND entity.UserId == CurrentUserId)
            if (entityType.ClrType == typeof(HrSystem.Domain.Entities.Employee.Employee))
            {
                // entity.Id == CurrentEmployeeId (when employee_id claim is in JWT)
                var entityId = Expression.Property(param, nameof(BaseEntity.Id));
                var empIdNotEmpty = Expression.NotEqual(currentEmpIdExpr, guidEmptyExpr);
                var empIdMatch = Expression.Equal(entityId, currentEmpIdExpr);
                var selfByEmpId = Expression.AndAlso(empIdNotEmpty, empIdMatch);

                // entity.UserId == CurrentUserId (fallback when employee_id claim is absent)
                var entityUserId = Expression.Property(param,
                    typeof(HrSystem.Domain.Entities.Employee.Employee).GetProperty("UserId")!);
                var userIdNotEmpty = Expression.NotEqual(currentUserIdExpr, guidEmptyExpr);
                var userIdMatch = Expression.Equal(
                    entityUserId,
                    Expression.Convert(currentUserIdExpr, typeof(Guid?)));
                var selfByUserId = Expression.AndAlso(userIdNotEmpty, userIdMatch);

                scopePredicate = Expression.OrElse(
                    scopePredicate,
                    Expression.OrElse(selfByEmpId, selfByUserId));
            }

            // Merge with the existing soft-delete filter (if any).
            var existingFilter = entityType.GetQueryFilter();
            Expression body = scopePredicate;

            if (existingFilter != null)
            {
                var existingBody = ReplacingExpressionVisitor.Replace(
                    existingFilter.Parameters[0], param, existingFilter.Body);
                body = Expression.AndAlso(existingBody, scopePredicate);
            }

            entityType.SetQueryFilter(Expression.Lambda(body, param));
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var now = DateTimeOffset.UtcNow;
        var userId = CurrentUser.Id;
        var organizationId = CurrentUser.OrganizationId;
        var branchId = CurrentUser.BranchId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            var entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entity.CreatedDate == default) entity.CreatedDate = now;
                    if (!entity.CreatedBy.HasValue && userId.HasValue) entity.CreatedBy = userId.Value;
                    if (entity.TenantId == Guid.Empty && organizationId.HasValue) entity.TenantId = organizationId.Value;

                    if (entity is Branch branchEntity)
                    {
                        if (!branchEntity.BranchId.HasValue || branchEntity.BranchId == Guid.Empty)
                        {
                            branchEntity.BranchId = branchEntity.Id;
                        }
                    }
                    else if ((!entity.BranchId.HasValue || entity.BranchId == Guid.Empty) && branchId.HasValue)
                    {
                        entity.BranchId = branchId.Value;
                    }

                    if (entity is BaseAuditableEntity auditableAdded)
                    {
                        auditableAdded.IsDeleted = false;
                    }
                    break;

                case EntityState.Modified:
                    entity.ModifiedDate = now;
                    if (userId.HasValue) entity.ModifiedBy = userId.Value;

                    entry.Property(nameof(BaseEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(BaseEntity.TenantId)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    if (entity is BaseAuditableEntity auditableDeleted)
                    {
                        entry.State = EntityState.Modified;
                        auditableDeleted.IsDeleted = true;
                        auditableDeleted.DeletedDate = now;
                        if (userId.HasValue) auditableDeleted.DeletedBy = userId.Value;
                    }
                    break;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseAuditableMasterEntity>())
        {
            var entity = entry.Entity;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entity.CreatedDate == default) entity.CreatedDate = now;
                    if (!entity.CreatedBy.HasValue && userId.HasValue) entity.CreatedBy = userId.Value;
                    entity.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    entity.ModifiedDate = now;
                    if (userId.HasValue) entity.ModifiedBy = userId.Value;

                    entry.Property(nameof(BaseAuditableMasterEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseAuditableMasterEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedDate = now;
                    if (userId.HasValue) entity.DeletedBy = userId.Value;
                    break;
            }
        }
    }


}
