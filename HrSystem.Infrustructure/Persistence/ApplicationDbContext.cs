using HrSystem.Domain.Common;
using HrSystem.Domain.Entities.Account;
using HrSystem.Infrustructure.Extensions;
using HrSystem.Shared.CurrentUser;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HrSystem.Infrustructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Identity
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserBranchRole> UserBranchRoles => Set<UserBranchRole>();

    // Employee Management
    public DbSet<HrSystem.Domain.Entities.Employee.Employee> Employees => Set<HrSystem.Domain.Entities.Employee.Employee>();
    public DbSet<HrSystem.Domain.Entities.Employee.Department> Departments => Set<HrSystem.Domain.Entities.Employee.Department>();
    public DbSet<HrSystem.Domain.Entities.Employee.JobTitle> JobTitles => Set<HrSystem.Domain.Entities.Employee.JobTitle>();
    public DbSet<HrSystem.Domain.Entities.Employee.EmployeeDocument> EmployeeDocuments => Set<HrSystem.Domain.Entities.Employee.EmployeeDocument>();
    public DbSet<HrSystem.Domain.Entities.Employee.EmployeeDocumentType> EmployeeDocumentTypes => Set<HrSystem.Domain.Entities.Employee.EmployeeDocumentType>();

    // Payroll
    public DbSet<HrSystem.Domain.Entities.Payroll.Salary> Salaries => Set<HrSystem.Domain.Entities.Payroll.Salary>();
    public DbSet<HrSystem.Domain.Entities.Payroll.AllowanceType> AllowanceTypes => Set<HrSystem.Domain.Entities.Payroll.AllowanceType>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryAllowance> SalaryAllowances => Set<HrSystem.Domain.Entities.Payroll.SalaryAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.DeductionType> DeductionTypes => Set<HrSystem.Domain.Entities.Payroll.DeductionType>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryDeduction> SalaryDeductions => Set<HrSystem.Domain.Entities.Payroll.SalaryDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.Loan> Loans => Set<HrSystem.Domain.Entities.Payroll.Loan>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayrollCycle> PayrollCycles => Set<HrSystem.Domain.Entities.Payroll.PayrollCycle>();
    public DbSet<HrSystem.Domain.Entities.Payroll.Payslip> Payslips => Set<HrSystem.Domain.Entities.Payroll.Payslip>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipAllowance> PayslipAllowances => Set<HrSystem.Domain.Entities.Payroll.PayslipAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipDeduction> PayslipDeductions => Set<HrSystem.Domain.Entities.Payroll.PayslipDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.TaxBracket> TaxBrackets => Set<HrSystem.Domain.Entities.Payroll.TaxBracket>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate> SocialInsuranceRates => Set<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate>();

    // Attendance
    public DbSet<HrSystem.Domain.Entities.Attendance.Attendance> Attendances => Set<HrSystem.Domain.Entities.Attendance.Attendance>();
    public DbSet<HrSystem.Domain.Entities.Attendance.EmployeeBiometric> EmployeeBiometrics => Set<HrSystem.Domain.Entities.Attendance.EmployeeBiometric>();
    public DbSet<HrSystem.Domain.Entities.Attendance.WorkSchedule> WorkSchedules => Set<HrSystem.Domain.Entities.Attendance.WorkSchedule>();
    public DbSet<HrSystem.Domain.Entities.Attendance.EmployeeWorkSchedule> EmployeeWorkSchedules => Set<HrSystem.Domain.Entities.Attendance.EmployeeWorkSchedule>();
    public DbSet<HrSystem.Domain.Entities.Attendance.PublicHoliday> PublicHolidays => Set<HrSystem.Domain.Entities.Attendance.PublicHoliday>();
    public DbSet<HrSystem.Domain.Entities.Attendance.OvertimeRequest> OvertimeRequests => Set<HrSystem.Domain.Entities.Attendance.OvertimeRequest>();
    public DbSet<HrSystem.Domain.Entities.Attendance.OvertimeStatus> OvertimeStatuses => Set<HrSystem.Domain.Entities.Attendance.OvertimeStatus>();

    // Leave Management
    public DbSet<HrSystem.Domain.Entities.Leave.LeavePolicy> LeavePolicies => Set<HrSystem.Domain.Entities.Leave.LeavePolicy>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveBalance> LeaveBalances => Set<HrSystem.Domain.Entities.Leave.LeaveBalance>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveRequest> LeaveRequests => Set<HrSystem.Domain.Entities.Leave.LeaveRequest>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveStatus> LeaveStatuses => Set<HrSystem.Domain.Entities.Leave.LeaveStatus>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveType> LeaveTypes => Set<HrSystem.Domain.Entities.Leave.LeaveType>();

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
    public DbSet<HrSystem.Domain.Entities.Lifecycle.OnboardingTask> OnboardingTasks => Set<HrSystem.Domain.Entities.Lifecycle.OnboardingTask>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.OffboardingTask> OffboardingTasks => Set<HrSystem.Domain.Entities.Lifecycle.OffboardingTask>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset> EmployeeAssets => Set<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.PolicyAcknowledgment> PolicyAcknowledgments => Set<HrSystem.Domain.Entities.Lifecycle.PolicyAcknowledgment>();

    // Organization & Multi-Tenancy
    public DbSet<HrSystem.Domain.Entities.Organization.Organization> Organizations => Set<HrSystem.Domain.Entities.Organization.Organization>();
    public DbSet<HrSystem.Domain.Entities.Organization.Branch> Branches => Set<HrSystem.Domain.Entities.Organization.Branch>();
    public DbSet<HrSystem.Domain.Entities.Organization.SubscriptionPlan> SubscriptionPlans => Set<HrSystem.Domain.Entities.Organization.SubscriptionPlan>();
    public DbSet<HrSystem.Domain.Entities.Organization.OrganizationInvoice> OrganizationInvoices => Set<HrSystem.Domain.Entities.Organization.OrganizationInvoice>();
    public DbSet<HrSystem.Domain.Entities.Organization.InvoiceStatus> InvoiceStatuses => Set<HrSystem.Domain.Entities.Organization.InvoiceStatus>();
    public DbSet<HrSystem.Domain.Entities.Organization.OrganizationInvoiceItem> OrganizationInvoiceItems => Set<HrSystem.Domain.Entities.Organization.OrganizationInvoiceItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.GetOnlyNotDeletedEntities();

        // Apply Multi-Tenancy + Branch Global Query Filters
        ApplyScopedFilters(modelBuilder);
    }

    private void ApplyScopedFilters(ModelBuilder modelBuilder)
    {
        // Get current organization ID from CurrentUser
        var organizationId = CurrentUser.OrganizationId;
        var isOrgAdmin = CurrentUser.IsOrganizationAdmin;
        var branchId = CurrentUser.BranchId;

        // Apply filter to all entities that inherit from BaseAuditableEntity (they have TenantId)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseAuditableEntity.TenantId));
                System.Linq.Expressions.Expression tenantPredicate;
                if (organizationId.HasValue)
                {
                    var organizationValue = System.Linq.Expressions.Expression.Constant(organizationId.Value);
                    tenantPredicate = System.Linq.Expressions.Expression.Equal(property, organizationValue);
                }
                else
                {
                    // No organization id -> allow all (e => true)
                    tenantPredicate = System.Linq.Expressions.Expression.Constant(true);
                }

                // Optional branch predicate when entity has BranchId and user is not org admin
                var branchProp = entityType.ClrType.GetProperty("BranchId");
                System.Linq.Expressions.Expression finalPredicate = tenantPredicate;

                if (!isOrgAdmin && branchProp != null && branchId.HasValue)
                {
                    var branchProperty = System.Linq.Expressions.Expression.Property(parameter, branchProp);
                    var branchConst = System.Linq.Expressions.Expression.Constant(branchId.Value, branchProp.PropertyType);

                    System.Linq.Expressions.Expression branchPredicate;
                    if (Nullable.GetUnderlyingType(branchProp.PropertyType) != null)
                    {
                        var nullableBranchConst = System.Linq.Expressions.Expression.Convert(branchConst, branchProp.PropertyType);
                        branchPredicate = System.Linq.Expressions.Expression.Equal(branchProperty, nullableBranchConst);
                    }
                    else
                    {
                        branchPredicate = System.Linq.Expressions.Expression.Equal(branchProperty, branchConst);
                    }

                    finalPredicate = System.Linq.Expressions.Expression.AndAlso(tenantPredicate, branchPredicate);
                }

                var lambda = System.Linq.Expressions.Expression.Lambda(finalPredicate, parameter);
                entityType.SetQueryFilter(lambda);
            }
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

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseAuditableEntity auditable) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (auditable.CreatedDate == default) auditable.CreatedDate = now;
                    if (auditable.CreatedBy == Guid.Empty && userId.HasValue) auditable.CreatedBy = userId.Value;
                    if (auditable.TenantId == Guid.Empty && organizationId.HasValue) auditable.TenantId = organizationId.Value;
                    // Auto-assign BranchId on entities that have it
                    var branchProp = entry.Entity.GetType().GetProperty("BranchId");
                    if (branchProp != null && branchId.HasValue)
                    {
                        var currentValue = branchProp.GetValue(entry.Entity);
                        var isDefault = currentValue is null || (currentValue is Guid g && g == Guid.Empty);
                        if (isDefault)
                        {
                            // Convert Guid to nullable Guid if needed
                            object valueToSet = branchId.Value;
                            if (Nullable.GetUnderlyingType(branchProp.PropertyType) != null)
                            {
                                valueToSet = (Guid?)branchId.Value;
                            }
                            branchProp.SetValue(entry.Entity, valueToSet);
                        }
                    }
                    auditable.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    auditable.ModifiedDate = now;
                    if (userId.HasValue) auditable.ModifiedBy = userId.Value;
                    entry.Property(nameof(BaseAuditableEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseAuditableEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(BaseAuditableEntity.TenantId)).IsModified = false;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    auditable.IsDeleted = true;
                    auditable.DeletedDate = now;
                    if (userId.HasValue) auditable.DeletedBy = userId.Value;
                    break;
            }
        }
    }


}
