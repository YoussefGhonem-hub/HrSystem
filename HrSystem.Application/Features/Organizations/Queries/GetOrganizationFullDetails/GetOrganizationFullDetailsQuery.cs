using ErrorOr;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationFullDetails;

/// <summary>
/// Returns comprehensive organization data for all Angular tabs:
/// - Company Info Tab
/// - Branches Tab  
/// - Structure Tab (Departments & Job Titles)
/// - Work Schedule Tab (per branch)
/// - Holidays Tab (per branch)
/// </summary>
public record GetOrganizationFullDetailsQuery(Guid Id) 
    : IRequest<ErrorOr<GenericResponse<OrganizationFullDetailsDto>>>;

#region Response DTOs

public record OrganizationFullDetailsDto
{
    // Company Info Tab
    public CompanyInfoDto CompanyInfo { get; init; } = new();

    // Users
    public OrganizationUserDto? AdminUser { get; init; }
    public OrganizationUserDto? HrManagerUser { get; init; }
    
    // Branches Tab
    public List<BranchFullDetailsDto> Branches { get; init; } = new();
    
    // Structure Tab
    public StructureDto Structure { get; init; } = new();
    
    // Summary stats
    public OrganizationStatsDto Stats { get; init; } = new();
}

public record OrganizationUserDto
{
    public Guid Id { get; init; }
    public string? FullName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? UserName { get; init; }
}

public record CompanyInfoDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? LogoUrl { get; init; }
    
    // Legal
    public string? CommercialRegistrationNumber { get; init; }
    public string? TaxRegistrationNumber { get; init; }
    public string? LegalEntityType { get; init; }
    
    // Contact
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? SecondaryPhoneNumber { get; init; }
    public string? Website { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    
    // Settings
    public string TimeZone { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string? WeekStartDay { get; init; }
    public string? DefaultLanguage { get; init; }
    
    // Subscription
    public bool IsActive { get; init; }
    public bool IsTrialPeriod { get; init; }
    public DateTime? TrialEndDate { get; init; }
    public DateTime SubscriptionStartDate { get; init; }
    public DateTime? SubscriptionEndDate { get; init; }
    public Guid? SubscriptionPlanId { get; init; }
    public string? SubscriptionPlanCode { get; init; }
    public string? SubscriptionPlanName { get; init; }
    public bool AllowPayrollModule { get; init; } = true;
    public bool AllowPerformanceModule { get; init; } = true;
    public bool AllowRecruitmentModule { get; init; } = true;
    public bool AllowCustomReports { get; init; } = true;
    public bool AllowBiometricIntegration { get; init; } = true;
    public bool AllowAPIAccess { get; init; } = true;
    
    // Limits
    public int MaxEmployees { get; init; }
    public int CurrentEmployeeCount { get; init; }
}

public record BranchFullDetailsDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    
    // Location
    public Guid CountryId { get; init; }
    public string? CountryName { get; init; }
    public string? City { get; init; }
    public string? AddressAr { get; init; }
    public string? AddressEn { get; init; }
    public string? PostalCode { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    
    // Contact
    public string? PhoneNumber { get; init; }
    public string? Email { get; init; }
    public string? Fax { get; init; }
    
    // Settings
    public string TimeZone { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public string? Language { get; init; }
    
    // Status
    public bool IsHeadquarter { get; init; }
    public bool IsActive { get; init; }
    public DateTime? OpeningDate { get; init; }
    
    // Work Schedules for this branch
    public List<WorkScheduleDetailsDto> WorkSchedules { get; init; } = new();

    // Holidays for this branch
    public List<HolidayDetailsDto> Holidays { get; init; } = new();
    
    // Stats
    public int EmployeeCount { get; init; }
    public int DepartmentCount { get; init; }
}

public record WorkScheduleDetailsDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeSpan StartTime { get; init; }
    public TimeSpan EndTime { get; init; }
    public TimeSpan? BreakDuration { get; init; }
    public int WorkingHoursPerDay { get; init; }
    public int WorkingDaysPerWeek { get; init; }
    public TimeSpan? GracePeriodLate { get; init; }
    public TimeSpan? GracePeriodEarlyLeave { get; init; }
    public bool IsSunday { get; init; }
    public bool IsMonday { get; init; }
    public bool IsTuesday { get; init; }
    public bool IsWednesday { get; init; }
    public bool IsThursday { get; init; }
    public bool IsFriday { get; init; }
    public bool IsSaturday { get; init; }
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public decimal ShiftTotalHours { get; init; }
    public decimal MinimumFullDayHours { get; init; }
    public decimal MinimumHalfDayHours { get; init; }
    public decimal AbsentThresholdHours { get; init; }
    public bool IsBreakTimeDeducted { get; init; }
    public int? CheckInWindowMinutes { get; init; }
    public bool IsOvertimeEnabled { get; init; }
    public decimal OvertimeStartsAfterHours { get; init; }
}

public record HolidayDetailsDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime Date { get; init; }
    public int Year { get; init; }
    public bool IsRecurring { get; init; }
    public int? RecurringMonth { get; init; }
    public int? RecurringDay { get; init; }
    public HolidayType Type { get; init; }
    public bool IsActive { get; init; }
}

public record StructureDto
{
    public List<DepartmentDetailsDto> Departments { get; init; } = new();
    public List<JobTitleDetailsDto> JobTitles { get; init; } = new();
}

public record DepartmentDetailsDto
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public string? ParentDepartmentName { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public int EmployeeCount { get; init; }
    public List<DepartmentDetailsDto> SubDepartments { get; init; } = new();
}

public record JobTitleDetailsDto
{
    public Guid Id { get; init; }
    public string TitleAr { get; init; } = string.Empty;
    public string TitleEn { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Level { get; init; }
    public decimal MinSalary { get; init; }
    public decimal MaxSalary { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public int EmployeeCount { get; init; }
}

public record OrganizationStatsDto
{
    public int TotalBranches { get; init; }
    public int ActiveBranches { get; init; }
    public int TotalDepartments { get; init; }
    public int TotalJobTitles { get; init; }
    public int TotalEmployees { get; init; }
    public int TotalHolidays { get; init; }
    public int TotalWorkSchedules { get; init; }
}

#endregion

#region Handler

public class GetOrganizationFullDetailsQueryHandler 
    : IRequestHandler<GetOrganizationFullDetailsQuery, ErrorOr<GenericResponse<OrganizationFullDetailsDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationFullDetailsQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationFullDetailsDto>>> Handle(
        GetOrganizationFullDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var defaultLanguage = LanguageDefaults.English;

        // Load organization with subscription plan
        var org = await _context.Organizations
            .AsNoTracking()
            .Include(o => o.SubscriptionPlan)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);

        if (org == null)
            return Error.NotFound("Organization.NotFound", "Organization not found");

        defaultLanguage = LanguageDefaults.NormalizeOrDefault(org.DefaultLanguage);

        // Load branches with countries, schedules, and holidays
        var branches = await _context.Branches
            .AsNoTracking()
            .Include(b => b.Country)
            .Include(b => b.WorkSchedules.Where(s => !s.IsDeleted))
            .Include(b => b.Holidays.Where(h => !h.IsDeleted))
            .Where(b => b.OrganizationId == request.Id && !b.IsDeleted)
            .OrderByDescending(b => b.IsHeadquarter)
            .ThenBy(b => b.NameEn)
            .ToListAsync(cancellationToken);

        // Load departments
        var departments = await _context.Departments
            .AsNoTracking()
            .Include(d => d.Branch)
            .Where(d => d.OrganizationId == request.Id && !d.IsDeleted)
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.NameEn)
            .ToListAsync(cancellationToken);

        // Load job titles
        var jobTitles = await _context.JobTitles
            .AsNoTracking()
            .Include(j => j.Branch)
            .Where(j => j.OrganizationId == request.Id && !j.IsDeleted)
            .OrderBy(j => j.SortOrder)
            .ThenBy(j => j.Level)
            .ThenBy(j => j.TitleEn)
            .ToListAsync(cancellationToken);

        // Get employee counts per department and job title
        var deptEmployeeCounts = await _context.Employees
            .Where(e => e.TenantId == request.Id && !e.IsDeleted && e.DepartmentId != null)
            .GroupBy(e => e.DepartmentId!.Value)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count, cancellationToken);

        var titleEmployeeCounts = await _context.Employees
            .Where(e => e.TenantId == request.Id && !e.IsDeleted && e.JobTitleId != null)
            .GroupBy(e => e.JobTitleId!.Value)
            .Select(g => new { JobTitleId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.JobTitleId, x => x.Count, cancellationToken);

        var branchEmployeeCounts = await _context.Employees
            .Where(e => e.TenantId == request.Id && !e.IsDeleted && e.BranchId != null)
            .GroupBy(e => e.BranchId!.Value)
            .Select(g => new { BranchId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BranchId, x => x.Count, cancellationToken);

        var totalEmployees = await _context.Employees
            .CountAsync(e => e.TenantId == request.Id && !e.IsDeleted, cancellationToken);

        var adminUser = await (from u in _context.Users.AsNoTracking()
                               join ur in _context.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                               join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                               where u.OrganizationId == request.Id && r.Name == Shared.Constants.RoleNames.OrganizationAdmin
                               orderby u.CreatedDate
                               select new OrganizationUserDto
                               {
                                   Id = u.Id,
                                   FullName = u.FullName,
                                   Email = u.Email ?? string.Empty,
                                   UserName = u.UserName
                               }).FirstOrDefaultAsync(cancellationToken);

        var hrManagerUser = await (from u in _context.Users.AsNoTracking()
                                   join ur in _context.UserRoles.AsNoTracking() on u.Id equals ur.UserId
                                   join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                                   where u.OrganizationId == request.Id && r.Name == Shared.Constants.RoleNames.HRManager
                                   orderby u.CreatedDate
                                   select new OrganizationUserDto
                                   {
                                       Id = u.Id,
                                       FullName = u.FullName,
                                       Email = u.Email ?? string.Empty,
                                       UserName = u.UserName
                                   }).FirstOrDefaultAsync(cancellationToken);

        // Build department hierarchy
        var deptLookup = departments.ToDictionary(d => d.Id);
        var rootDepartments = departments.Where(d => d.ParentDepartmentId == null).ToList();

        List<DepartmentDetailsDto> BuildDepartmentTree(List<Domain.Entities.Employee.Department> depts)
        {
            return depts.Select(d => new DepartmentDetailsDto
            {
                Id = d.Id,
                NameAr = d.NameAr,
                NameEn = d.NameEn,
                Code = d.Code,
                Description = d.Description,
                ParentDepartmentId = d.ParentDepartmentId,
                ParentDepartmentName = d.ParentDepartmentId.HasValue && deptLookup.ContainsKey(d.ParentDepartmentId.Value) 
                    ? deptLookup[d.ParentDepartmentId.Value].NameEn 
                    : null,
                BranchId = d.BranchId,
                BranchName = d.Branch?.NameEn,
                SortOrder = d.SortOrder,
                IsActive = d.IsActive,
                EmployeeCount = deptEmployeeCounts.GetValueOrDefault(d.Id, 0),
                SubDepartments = BuildDepartmentTree(departments.Where(sd => sd.ParentDepartmentId == d.Id).ToList())
            }).ToList();
        }

        // Build response
        var dto = new OrganizationFullDetailsDto
        {
            CompanyInfo = new CompanyInfoDto
            {
                Id = org.Id,
                Code = org.Code,
                NameAr = org.NameAr,
                NameEn = org.NameEn,
                Industry = org.Industry,
                LogoUrl = org.LogoUrl,
                CommercialRegistrationNumber = org.CommercialRegistrationNumber,
                TaxRegistrationNumber = org.TaxRegistrationNumber,
                LegalEntityType = org.LegalEntityType,
                Email = org.Email,
                PhoneNumber = org.PhoneNumber,
                SecondaryPhoneNumber = org.SecondaryPhoneNumber,
                Website = org.Website,
                AddressAr = org.AddressAr,
                AddressEn = org.AddressEn,
                City = org.City,
                Country = org.Country,
                PostalCode = org.PostalCode,
                TimeZone = org.TimeZone,
                Currency = org.Currency,
                WeekStartDay = org.WeekStartDay,
                DefaultLanguage = defaultLanguage,
                IsActive = org.IsActive,
                IsTrialPeriod = org.IsTrialPeriod,
                TrialEndDate = org.TrialEndDate,
                SubscriptionStartDate = org.SubscriptionStartDate,
                SubscriptionEndDate = org.SubscriptionEndDate,
                SubscriptionPlanId = org.SubscriptionPlanId,
                SubscriptionPlanCode = org.SubscriptionPlan?.Code,
                SubscriptionPlanName = org.SubscriptionPlan?.NameEn,
                AllowPayrollModule = org.SubscriptionPlan?.AllowPayrollModule ?? true,
                AllowPerformanceModule = org.SubscriptionPlan?.AllowPerformanceModule ?? true,
                AllowRecruitmentModule = org.SubscriptionPlan?.AllowRecruitmentModule ?? true,
                AllowCustomReports = org.SubscriptionPlan?.AllowCustomReports ?? true,
                AllowBiometricIntegration = org.SubscriptionPlan?.AllowBiometricIntegration ?? true,
                AllowAPIAccess = org.SubscriptionPlan?.AllowAPIAccess ?? true,
                MaxEmployees = org.MaxEmployees,
                CurrentEmployeeCount = totalEmployees
            },
            AdminUser = adminUser,
            HrManagerUser = hrManagerUser,
            Branches = branches.Select(b => new BranchFullDetailsDto
            {
                Id = b.Id,
                NameAr = b.NameAr,
                NameEn = b.NameEn,
                Code = b.Code,
                Description = b.Description,
                CountryId = b.CountryId,
                CountryName = b.Country?.NameEn,
                City = b.City,
                AddressAr = b.AddressAr,
                AddressEn = b.AddressEn,
                PostalCode = b.PostalCode,
                Latitude = b.Latitude,
                Longitude = b.Longitude,
                PhoneNumber = b.PhoneNumber,
                Email = b.Email,
                Fax = b.Fax,
                TimeZone = b.TimeZone,
                Currency = b.Currency,
                Language = LanguageDefaults.NormalizeOrDefault(b.Language, defaultLanguage),
                IsHeadquarter = b.IsHeadquarter,
                IsActive = b.IsActive,
                OpeningDate = b.OpeningDate,
                EmployeeCount = branchEmployeeCounts.GetValueOrDefault(b.Id, 0),
                DepartmentCount = departments.Count(d => d.BranchId == b.Id),
                WorkSchedules = b.WorkSchedules
                    .OrderByDescending(s => s.IsDefault)
                    .ThenBy(s => s.Name)
                    .Select(s => new WorkScheduleDetailsDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        BreakDuration = s.BreakDuration,
                        WorkingHoursPerDay = s.WorkingHoursPerDay,
                        WorkingDaysPerWeek = s.WorkingDaysPerWeek,
                        GracePeriodLate = s.GracePeriodLate,
                        GracePeriodEarlyLeave = s.GracePeriodEarlyLeave,
                        IsSunday = s.IsSunday,
                        IsMonday = s.IsMonday,
                        IsTuesday = s.IsTuesday,
                        IsWednesday = s.IsWednesday,
                        IsThursday = s.IsThursday,
                        IsFriday = s.IsFriday,
                        IsSaturday = s.IsSaturday,
                        IsDefault = s.IsDefault,
                        IsActive = s.IsActive,
                        TimeZone = s.TimeZone,
                        ShiftTotalHours = s.ShiftTotalHours,
                        MinimumFullDayHours = s.MinimumFullDayHours,
                        MinimumHalfDayHours = s.MinimumHalfDayHours,
                        AbsentThresholdHours = s.AbsentThresholdHours,
                        IsBreakTimeDeducted = s.IsBreakTimeDeducted,
                        CheckInWindowMinutes = s.CheckInWindowMinutes,
                        IsOvertimeEnabled = s.IsOvertimeEnabled,
                        OvertimeStartsAfterHours = s.OvertimeStartsAfterHours
                    }).ToList(),
                Holidays = b.Holidays
                    .OrderBy(h => h.Date)
                    .Select(h => new HolidayDetailsDto
                    {
                        Id = h.Id,
                        NameAr = h.NameAr,
                        NameEn = h.NameEn,
                        Description = h.Description,
                        Date = h.Date,
                        Year = h.Year,
                        IsRecurring = h.IsRecurring,
                        RecurringMonth = h.RecurringMonth,
                        RecurringDay = h.RecurringDay,
                        Type = h.Type,
                        IsActive = h.IsActive
                    }).ToList()
            }).ToList(),
            Structure = new StructureDto
            {
                Departments = BuildDepartmentTree(rootDepartments),
                JobTitles = jobTitles.Select(j => new JobTitleDetailsDto
                {
                    Id = j.Id,
                    TitleAr = j.TitleAr,
                    TitleEn = j.TitleEn,
                    Code = j.Code,
                    Description = j.Description,
                    Level = j.Level,
                    MinSalary = j.MinSalary,
                    MaxSalary = j.MaxSalary,
                    BranchId = j.BranchId,
                    BranchName = j.Branch?.NameEn,
                    SortOrder = j.SortOrder,
                    IsActive = j.IsActive,
                    EmployeeCount = titleEmployeeCounts.GetValueOrDefault(j.Id, 0)
                }).ToList()
            },
            Stats = new OrganizationStatsDto
            {
                TotalBranches = branches.Count,
                ActiveBranches = branches.Count(b => b.IsActive),
                TotalDepartments = departments.Count,
                TotalJobTitles = jobTitles.Count,
                TotalEmployees = totalEmployees,
                TotalHolidays = branches.Sum(b => b.Holidays.Count),
                TotalWorkSchedules = branches.Sum(b => b.WorkSchedules.Count)
            }
        };

        return new GenericResponse<OrganizationFullDetailsDto>
        {
            Success = true,
            Data = dto
        };
    }
}

#endregion
