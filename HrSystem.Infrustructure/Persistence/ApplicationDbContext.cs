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
    
    // Employee Management
    public DbSet<HrSystem.Domain.Entities.Employee.Employee> Employees => Set<HrSystem.Domain.Entities.Employee.Employee>();
    public DbSet<HrSystem.Domain.Entities.Employee.Department> Departments => Set<HrSystem.Domain.Entities.Employee.Department>();
    public DbSet<HrSystem.Domain.Entities.Employee.JobTitle> JobTitles => Set<HrSystem.Domain.Entities.Employee.JobTitle>();
    public DbSet<HrSystem.Domain.Entities.Employee.EmployeeDocument> EmployeeDocuments => Set<HrSystem.Domain.Entities.Employee.EmployeeDocument>();
    
    // Payroll
    public DbSet<HrSystem.Domain.Entities.Payroll.Salary> Salaries => Set<HrSystem.Domain.Entities.Payroll.Salary>();
    public DbSet<HrSystem.Domain.Entities.Payroll.AllowanceType> AllowanceTypes => Set<HrSystem.Domain.Entities.Payroll.AllowanceType>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryAllowance> SalaryAllowances => Set<HrSystem.Domain.Entities.Payroll.SalaryAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.DeductionType> DeductionTypes => Set<HrSystem.Domain.Entities.Payroll.DeductionType>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SalaryDeduction> SalaryDeductions => Set<HrSystem.Domain.Entities.Payroll.SalaryDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayrollCycle> PayrollCycles => Set<HrSystem.Domain.Entities.Payroll.PayrollCycle>();
    public DbSet<HrSystem.Domain.Entities.Payroll.Payslip> Payslips => Set<HrSystem.Domain.Entities.Payroll.Payslip>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipAllowance> PayslipAllowances => Set<HrSystem.Domain.Entities.Payroll.PayslipAllowance>();
    public DbSet<HrSystem.Domain.Entities.Payroll.PayslipDeduction> PayslipDeductions => Set<HrSystem.Domain.Entities.Payroll.PayslipDeduction>();
    public DbSet<HrSystem.Domain.Entities.Payroll.TaxBracket> TaxBrackets => Set<HrSystem.Domain.Entities.Payroll.TaxBracket>();
    public DbSet<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate> SocialInsuranceRates => Set<HrSystem.Domain.Entities.Payroll.SocialInsuranceRate>();
    
    // Attendance
    public DbSet<HrSystem.Domain.Entities.Attendance.Attendance> Attendances => Set<HrSystem.Domain.Entities.Attendance.Attendance>();
    public DbSet<HrSystem.Domain.Entities.Attendance.WorkSchedule> WorkSchedules => Set<HrSystem.Domain.Entities.Attendance.WorkSchedule>();
    public DbSet<HrSystem.Domain.Entities.Attendance.EmployeeWorkSchedule> EmployeeWorkSchedules => Set<HrSystem.Domain.Entities.Attendance.EmployeeWorkSchedule>();
    public DbSet<HrSystem.Domain.Entities.Attendance.PublicHoliday> PublicHolidays => Set<HrSystem.Domain.Entities.Attendance.PublicHoliday>();
    public DbSet<HrSystem.Domain.Entities.Attendance.OvertimeRequest> OvertimeRequests => Set<HrSystem.Domain.Entities.Attendance.OvertimeRequest>();
    
    // Leave Management
    public DbSet<HrSystem.Domain.Entities.Leave.LeavePolicy> LeavePolicies => Set<HrSystem.Domain.Entities.Leave.LeavePolicy>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveBalance> LeaveBalances => Set<HrSystem.Domain.Entities.Leave.LeaveBalance>();
    public DbSet<HrSystem.Domain.Entities.Leave.LeaveRequest> LeaveRequests => Set<HrSystem.Domain.Entities.Leave.LeaveRequest>();
    
    // Performance Management
    public DbSet<HrSystem.Domain.Entities.Performance.PerformanceReview> PerformanceReviews => Set<HrSystem.Domain.Entities.Performance.PerformanceReview>();
    public DbSet<HrSystem.Domain.Entities.Performance.KPI> KPIs => Set<HrSystem.Domain.Entities.Performance.KPI>();
    public DbSet<HrSystem.Domain.Entities.Performance.KPIEvaluation> KPIEvaluations => Set<HrSystem.Domain.Entities.Performance.KPIEvaluation>();
    public DbSet<HrSystem.Domain.Entities.Performance.Goal> Goals => Set<HrSystem.Domain.Entities.Performance.Goal>();
    public DbSet<HrSystem.Domain.Entities.Performance.GoalMilestone> GoalMilestones => Set<HrSystem.Domain.Entities.Performance.GoalMilestone>();
    public DbSet<HrSystem.Domain.Entities.Performance.Feedback> Feedbacks => Set<HrSystem.Domain.Entities.Performance.Feedback>();
    
    // Lifecycle Management
    public DbSet<HrSystem.Domain.Entities.Lifecycle.OnboardingTask> OnboardingTasks => Set<HrSystem.Domain.Entities.Lifecycle.OnboardingTask>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.OffboardingTask> OffboardingTasks => Set<HrSystem.Domain.Entities.Lifecycle.OffboardingTask>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset> EmployeeAssets => Set<HrSystem.Domain.Entities.Lifecycle.EmployeeAsset>();
    public DbSet<HrSystem.Domain.Entities.Lifecycle.PolicyAcknowledgment> PolicyAcknowledgments => Set<HrSystem.Domain.Entities.Lifecycle.PolicyAcknowledgment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.GetOnlyNotDeletedEntities();
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

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not BaseAuditableEntity auditable) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (auditable.CreatedDate == default) auditable.CreatedDate = now;
                    if (auditable.CreatedBy == Guid.Empty && userId.HasValue) auditable.CreatedBy = userId.Value;
                    auditable.IsDeleted = false;
                    break;

                case EntityState.Modified:
                    auditable.ModifiedDate = now;
                    if (userId.HasValue) auditable.ModifiedBy = userId.Value;
                    entry.Property(nameof(BaseAuditableEntity.CreatedDate)).IsModified = false;
                    entry.Property(nameof(BaseAuditableEntity.CreatedBy)).IsModified = false;
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
