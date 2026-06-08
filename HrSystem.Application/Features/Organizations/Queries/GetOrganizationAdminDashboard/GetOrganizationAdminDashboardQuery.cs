using ErrorOr;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Queries.GetOrganizationAdminDashboard;

public record GetOrganizationAdminDashboardQuery(
    DateTime? Date = null,
    int EmployeePageNumber = 1,
    int EmployeePageSize = 20,
    string? SearchTerm = null,
    int RecentAttendanceCount = 50,
    int RecentLeaveHistoryCount = 20,
    int RecentLeaveRequestsCount = 20
) : IRequest<ErrorOr<GenericResponse<OrganizationAdminDashboardDto>>>;

public record OrganizationAdminDashboardDto
{
    public DateTime Date { get; init; }
    public EmployeeOverviewDto EmployeeOverview { get; init; } = new();
    public PagedResult<EmployeeProfileDto> EmployeeProfiles { get; init; } = PagedResult<EmployeeProfileDto>.Create(Array.Empty<EmployeeProfileDto>(), 0, 1, 20);
    public OrganizationalStructureDto OrganizationalStructure { get; init; } = new();
    public AttendanceAndLeaveDto AttendanceAndLeave { get; init; } = new();
}

public record EmployeeOverviewDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int OnLeaveEmployees { get; init; }
    public int TerminatedEmployees { get; init; }
}

public record EmployeeProfileDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string FullNameAr { get; init; } = string.Empty;
    public string? DepartmentName { get; init; }
    public string? JobTitleName { get; init; }
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? AddressEn { get; init; }
    public string AddressAr { get; init; } = string.Empty;
    public Guid StatusId { get; init; }
    public string EmploymentStatus { get; init; } = string.Empty;
}

public record OrganizationalStructureDto
{
    public List<DepartmentTeamDto> Departments { get; init; } = new();
    public List<JobRolePositionDto> JobRoles { get; init; } = new();
    public List<ReportingHierarchyDto> ReportingHierarchy { get; init; } = new();
}

public record DepartmentTeamDto
{
    public Guid DepartmentId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid? ParentDepartmentId { get; init; }
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public int EmployeesCount { get; init; }
    public int SubDepartmentsCount { get; init; }
}

public record JobRolePositionDto
{
    public Guid JobTitleId { get; init; }
    public string TitleEn { get; init; } = string.Empty;
    public string TitleAr { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int Level { get; init; }
    public bool IsActive { get; init; }
    public int EmployeesCount { get; init; }
}

public record ReportingHierarchyDto
{
    public Guid ManagerEmployeeId { get; init; }
    public string ManagerName { get; init; } = string.Empty;
    public string? ManagerDepartment { get; init; }
    public int DirectReportsCount { get; init; }
    public List<ReportingEmployeeDto> DirectReports { get; init; } = new();
}

public record ReportingEmployeeDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string? DepartmentName { get; init; }
    public string? JobTitleName { get; init; }
}

public record AttendanceAndLeaveDto
{
    public DailyAttendanceSummaryDto DailyAttendanceSummary { get; init; } = new();
    public List<DailyAttendanceRecordDto> DailyAttendanceRecords { get; init; } = new();
    public LeaveBalanceSummaryDto LeaveBalanceSummary { get; init; } = new();
    public List<EmployeeLeaveBalanceDto> LeaveBalances { get; init; } = new();
    public List<LeaveHistoryDto> LeaveHistory { get; init; } = new();
    public LeaveRequestApprovalSummaryDto LeaveRequestSummary { get; init; } = new();
    public List<LeaveRequestDto> RecentLeaveRequests { get; init; } = new();
}

public record DailyAttendanceSummaryDto
{
    public int TotalRecords { get; init; }
    public int PresentCount { get; init; }
    public int AbsentCount { get; init; }
    public int OnLeaveCount { get; init; }
    public int LateCount { get; init; }
}

public record DailyAttendanceRecordDto
{
    public Guid AttendanceId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public string? DepartmentName { get; init; }
    public DateTime Date { get; init; }
    public TimeSpan? CheckInTime { get; init; }
    public TimeSpan? CheckOutTime { get; init; }
    public bool IsLate { get; init; }
    public bool IsEarlyLeave { get; init; }
    public bool IsOvertime { get; init; }
    public string AttendanceStatus { get; init; } = string.Empty;
}

public record LeaveBalanceSummaryDto
{
    public decimal TotalAllocatedDays { get; init; }
    public decimal TotalUsedDays { get; init; }
    public decimal TotalAvailableDays { get; init; }
}

public record EmployeeLeaveBalanceDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = string.Empty;
    public string EmployeeName { get; init; } = string.Empty;
    public Guid VacationTypeId { get; init; }
    public string VacationType { get; init; } = string.Empty;
    public int Year { get; init; }
    public decimal AllocatedDays { get; init; }
    public decimal CarryOverDays { get; init; }
    public decimal ManualAdjustmentDays { get; init; }
    public decimal UsedDays { get; init; }
    public decimal AvailableDays { get; init; }
}

public record LeaveHistoryDto
{
    public Guid TransactionId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string VacationType { get; init; } = string.Empty;
    public int Year { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public decimal DaysChanged { get; init; }
    public decimal BalanceAfter { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTimeOffset CreatedDate { get; init; }
}

public record LeaveRequestApprovalSummaryDto
{
    public int TotalLeaveRequests { get; init; }
    public int PendingRequests { get; init; }
    public int ManagerApprovedRequests { get; init; }
    public int ApprovedRequests { get; init; }
    public int RejectedRequests { get; init; }
}

public record LeaveRequestDto
{
    public Guid RequestId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public string RequestType { get; init; } = string.Empty;
    public EmployeeRequestStatus Status { get; init; }
    public DateTime RequestedDate { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public Guid? ApprovedBy { get; init; }
    public DateTime? ApprovedDate { get; init; }
}

public class GetOrganizationAdminDashboardQueryHandler
    : IRequestHandler<GetOrganizationAdminDashboardQuery, ErrorOr<GenericResponse<OrganizationAdminDashboardDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOrganizationAdminDashboardQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<OrganizationAdminDashboardDto>>> Handle(
        GetOrganizationAdminDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var organizationId = CurrentUser.OrganizationId;
        if (!organizationId.HasValue || organizationId.Value == Guid.Empty)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var date = request.Date?.Date ?? DateTime.UtcNow.Date;
        var year = date.Year;
        var scopedEmployeeIds = await ResolveScopedEmployeeIds(cancellationToken);

        var employeeOverview = await BuildEmployeeOverview(scopedEmployeeIds, cancellationToken);
        var employeeProfiles = await BuildEmployeeProfiles(request, scopedEmployeeIds, cancellationToken);
        var organizationalStructure = await BuildOrganizationalStructure(scopedEmployeeIds, cancellationToken);
        var attendanceAndLeave = await BuildAttendanceAndLeave(date, year, request, scopedEmployeeIds, cancellationToken);

        var dto = new OrganizationAdminDashboardDto
        {
            Date = date,
            EmployeeOverview = employeeOverview,
            EmployeeProfiles = employeeProfiles,
            OrganizationalStructure = organizationalStructure,
            AttendanceAndLeave = attendanceAndLeave
        };

        return GenericResponse<OrganizationAdminDashboardDto>.SuccessResult(
            dto,
            "Organization admin dashboard retrieved successfully");
    }

    private async Task<HashSet<Guid>?> ResolveScopedEmployeeIds(CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles.Select(RoleNames.Normalize).ToArray();
        var isDepartmentManager = roles.Any(r => string.Equals(r, RoleNames.DepartmentManager, StringComparison.OrdinalIgnoreCase));
        var hasElevatedDashboardAccess =
            roles.Any(r => string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)) ||
            roles.Any(r => string.Equals(r, RoleNames.OrganizationAdmin, StringComparison.OrdinalIgnoreCase)) ||
            roles.Any(r => string.Equals(r, RoleNames.HRManager, StringComparison.OrdinalIgnoreCase));

        if (!isDepartmentManager || hasElevatedDashboardAccess)
        {
            return null;
        }

        var managerEmployeeId = CurrentUser.EmployeeId;
        if (!managerEmployeeId.HasValue || managerEmployeeId.Value == Guid.Empty)
        {
            return new HashSet<Guid>();
        }

        var managerDepartmentId = await _context.Employees
            .AsNoTracking()
            .Where(e => e.Id == managerEmployeeId.Value)
            .Select(e => e.DepartmentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (managerDepartmentId.HasValue)
        {
            var departmentEmployees = await _context.Employees
                .AsNoTracking()
                .Where(e => e.DepartmentId == managerDepartmentId.Value)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            if (departmentEmployees.Count > 0)
            {
                return departmentEmployees.ToHashSet();
            }
        }

        var directReports = await _context.Employees
            .AsNoTracking()
            .Where(e => e.DirectManagerId == managerEmployeeId.Value)
            .Select(e => e.Id)
            .ToListAsync(cancellationToken);

        return directReports.ToHashSet();
    }

    private async Task<EmployeeOverviewDto> BuildEmployeeOverview(
        HashSet<Guid>? scopedEmployeeIds,
        CancellationToken cancellationToken)
    {
        var employees = _context.Employees.AsNoTracking().AsQueryable();
        if (scopedEmployeeIds is not null)
        {
            employees = employees.Where(e => scopedEmployeeIds.Contains(e.Id));
        }

        var totalEmployees = await employees.CountAsync(cancellationToken);
        var activeEmployees = await employees.CountAsync(e => e.StatusId == EmployeeStatusIds.Active, cancellationToken);
        var onLeaveEmployees = await employees.CountAsync(e => e.StatusId == EmployeeStatusIds.OnLeave, cancellationToken);
        var terminatedEmployees = await employees.CountAsync(e => e.StatusId == EmployeeStatusIds.Terminated, cancellationToken);

        return new EmployeeOverviewDto
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            OnLeaveEmployees = onLeaveEmployees,
            TerminatedEmployees = terminatedEmployees
        };
    }

    private async Task<PagedResult<EmployeeProfileDto>> BuildEmployeeProfiles(
        GetOrganizationAdminDashboardQuery request,
        HashSet<Guid>? scopedEmployeeIds,
        CancellationToken cancellationToken)
    {
        var employees = _context.Employees
            .AsNoTracking()
            .AsQueryable();

        if (scopedEmployeeIds is not null)
        {
            employees = employees.Where(e => scopedEmployeeIds.Contains(e.Id));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            employees = employees.Where(e =>
                e.EmployeeCode.Contains(term) ||
                e.FirstNameEn.Contains(term) ||
                e.LastNameEn.Contains(term) ||
                e.FirstNameAr.Contains(term) ||
                e.LastNameAr.Contains(term) ||
                e.Email.Contains(term) ||
                e.PhoneNumber.Contains(term));
        }

        var projected = employees
            .OrderBy(e => e.FirstNameEn)
            .ThenBy(e => e.LastNameEn)
            .Select(e => new EmployeeProfileDto
            {
                EmployeeId = e.Id,
                EmployeeCode = e.EmployeeCode,
                FullNameEn = e.FullNameEn,
                FullNameAr = e.FullNameAr,
                DepartmentName = e.Department != null ? e.Department.NameEn : null,
                JobTitleName = e.JobTitle != null ? e.JobTitle.TitleEn : null,
                Email = e.Email,
                PhoneNumber = e.PhoneNumber,
                AddressEn = e.AddressEn,
                AddressAr = e.AddressAr,
                StatusId = e.StatusId,
                EmploymentStatus = e.Status != null ? e.Status.NameEn : string.Empty
            });

        return await projected.ToPagedResultAsync(request.EmployeePageNumber, request.EmployeePageSize, cancellationToken);
    }

    private async Task<OrganizationalStructureDto> BuildOrganizationalStructure(
        HashSet<Guid>? scopedEmployeeIds,
        CancellationToken cancellationToken)
    {
        var departmentsQuery = _context.Departments
            .AsNoTracking();

        if (scopedEmployeeIds is not null)
        {
            departmentsQuery = departmentsQuery
                .Where(d => d.Employees.Any(e => scopedEmployeeIds.Contains(e.Id)));
        }

        var departments = await departmentsQuery
            .AsNoTracking()
            .OrderBy(d => d.SortOrder)
            .ThenBy(d => d.NameEn)
            .Select(d => new DepartmentTeamDto
            {
                DepartmentId = d.Id,
                NameEn = d.NameEn,
                NameAr = d.NameAr,
                Code = d.Code,
                IsActive = d.IsActive,
                ParentDepartmentId = d.ParentDepartmentId,
                ManagerId = d.ManagerId,
                ManagerName = d.Manager != null ? d.Manager.FullNameEn : null,
                EmployeesCount = scopedEmployeeIds == null
                    ? d.Employees.Count
                    : d.Employees.Count(e => scopedEmployeeIds.Contains(e.Id)),
                SubDepartmentsCount = d.SubDepartments.Count(sd => sd.IsActive)
            })
            .ToListAsync(cancellationToken);

        var jobRolesQuery = _context.JobTitles
            .AsNoTracking();

        if (scopedEmployeeIds is not null)
        {
            jobRolesQuery = jobRolesQuery
                .Where(j => j.Employees.Any(e => scopedEmployeeIds.Contains(e.Id)));
        }

        var jobRoles = await jobRolesQuery
            .AsNoTracking()
            .OrderBy(j => j.Level)
            .ThenBy(j => j.TitleEn)
            .Select(j => new JobRolePositionDto
            {
                JobTitleId = j.Id,
                TitleEn = j.TitleEn,
                TitleAr = j.TitleAr,
                Code = j.Code,
                Level = j.Level,
                IsActive = j.IsActive,
                EmployeesCount = scopedEmployeeIds == null
                    ? j.Employees.Count
                    : j.Employees.Count(e => scopedEmployeeIds.Contains(e.Id))
            })
            .ToListAsync(cancellationToken);

        var managerIds = await _context.Employees
            .AsNoTracking()
            .Where(e => e.DirectManagerId.HasValue)
            .Select(e => e.DirectManagerId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var managers = await _context.Employees
            .AsNoTracking()
            .Where(e => managerIds.Contains(e.Id))
            .Select(e => new
            {
                e.Id,
                Name = e.FullNameEn,
                DepartmentName = e.Department != null ? e.Department.NameEn : null
            })
            .ToListAsync(cancellationToken);

        var directReports = await _context.Employees
            .AsNoTracking()
            .Where(e => e.DirectManagerId.HasValue)
            .Where(e => scopedEmployeeIds == null || scopedEmployeeIds.Contains(e.Id))
            .Select(e => new
            {
                ManagerId = e.DirectManagerId!.Value,
                EmployeeId = e.Id,
                e.EmployeeCode,
                Name = e.FullNameEn,
                DepartmentName = e.Department != null ? e.Department.NameEn : null,
                JobTitleName = e.JobTitle != null ? e.JobTitle.TitleEn : null
            })
            .ToListAsync(cancellationToken);

        if (scopedEmployeeIds is not null)
        {
            managerIds = directReports
                .Select(r => r.ManagerId)
                .Distinct()
                .ToList();

            managers = await _context.Employees
                .AsNoTracking()
                .Where(e => managerIds.Contains(e.Id))
                .Select(e => new
                {
                    e.Id,
                    Name = e.FullNameEn,
                    DepartmentName = e.Department != null ? e.Department.NameEn : null
                })
                .ToListAsync(cancellationToken);
        }

        var reportingHierarchy = managers
            .Select(m =>
            {
                var reports = directReports
                    .Where(r => r.ManagerId == m.Id)
                    .OrderBy(r => r.Name)
                    .ToList();

                return new ReportingHierarchyDto
                {
                    ManagerEmployeeId = m.Id,
                    ManagerName = m.Name,
                    ManagerDepartment = m.DepartmentName,
                    DirectReportsCount = reports.Count,
                    DirectReports = reports
                        .Take(10)
                        .Select(r => new ReportingEmployeeDto
                        {
                            EmployeeId = r.EmployeeId,
                            EmployeeCode = r.EmployeeCode,
                            FullNameEn = r.Name,
                            DepartmentName = r.DepartmentName,
                            JobTitleName = r.JobTitleName
                        })
                        .ToList()
                };
            })
            .OrderByDescending(h => h.DirectReportsCount)
            .ThenBy(h => h.ManagerName)
            .ToList();

        return new OrganizationalStructureDto
        {
            Departments = departments,
            JobRoles = jobRoles,
            ReportingHierarchy = reportingHierarchy
        };
    }

    private async Task<AttendanceAndLeaveDto> BuildAttendanceAndLeave(
        DateTime date,
        int year,
        GetOrganizationAdminDashboardQuery request,
        HashSet<Guid>? scopedEmployeeIds,
        CancellationToken cancellationToken)
    {
        var safeAttendanceCount = request.RecentAttendanceCount <= 0 ? 50 : request.RecentAttendanceCount;
        var safeLeaveHistoryCount = request.RecentLeaveHistoryCount <= 0 ? 20 : request.RecentLeaveHistoryCount;
        var safeLeaveRequestsCount = request.RecentLeaveRequestsCount <= 0 ? 20 : request.RecentLeaveRequestsCount;

        var attendanceQuery = _context.Attendances
            .AsNoTracking()
            .Where(a => a.Date.Date == date);

        if (scopedEmployeeIds is not null)
        {
            attendanceQuery = attendanceQuery.Where(a => scopedEmployeeIds.Contains(a.EmployeeId));
        }

        var totalRecords = await attendanceQuery.CountAsync(cancellationToken);
        var presentCount = await attendanceQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Present, cancellationToken);
        var explicitAbsent = await attendanceQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Absent, cancellationToken);
        var onLeaveCount = await attendanceQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.OnLeave, cancellationToken);
        var lateCount = await attendanceQuery.CountAsync(a => a.IsLate, cancellationToken);

        // Employees with no record today are implicitly absent.
        var activeEmployeesQuery = _context.Employees.AsNoTracking()
            .Where(e => e.StatusId == EmployeeStatusIds.Active);
        if (scopedEmployeeIds is not null)
            activeEmployeesQuery = activeEmployeesQuery.Where(e => scopedEmployeeIds.Contains(e.Id));
        var totalActive = await activeEmployeesQuery.CountAsync(cancellationToken);
        var employeesWithRecordToday = await attendanceQuery.Select(a => a.EmployeeId).Distinct().CountAsync(cancellationToken);
        var absentCount = explicitAbsent + Math.Max(0, totalActive - employeesWithRecordToday);

        var dailySummary = new DailyAttendanceSummaryDto
        {
            TotalRecords = totalRecords,
            PresentCount = presentCount,
            AbsentCount = absentCount,
            OnLeaveCount = onLeaveCount,
            LateCount = lateCount
        };

        var dailyRecords = await attendanceQuery
            .OrderBy(a => a.Employee.FirstNameEn)
            .ThenBy(a => a.Employee.LastNameEn)
            .Take(safeAttendanceCount)
            .Select(a => new DailyAttendanceRecordDto
            {
                AttendanceId = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeCode = a.Employee.EmployeeCode,
                EmployeeName = a.Employee.FullNameEn,
                DepartmentName = a.Employee.Department != null ? a.Employee.Department.NameEn : null,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                IsLate = a.IsLate,
                IsEarlyLeave = a.IsEarlyLeave,
                IsOvertime = a.IsOvertime,
                AttendanceStatus = a.Status != null ? a.Status.NameEn : string.Empty
            })
            .ToListAsync(cancellationToken);

        var leaveBalances = await _context.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(lb => lb.Year == year)
            .Where(lb => scopedEmployeeIds == null || scopedEmployeeIds.Contains(lb.EmployeeId))
            .OrderBy(lb => lb.Employee.FirstNameEn)
            .ThenBy(lb => lb.VacationType.NameEn)
            .Select(lb => new EmployeeLeaveBalanceDto
            {
                EmployeeId = lb.EmployeeId,
                EmployeeCode = lb.Employee.EmployeeCode,
                EmployeeName = lb.Employee.FullNameEn,
                VacationTypeId = lb.VacationTypeId,
                VacationType = lb.VacationType.NameEn,
                Year = lb.Year,
                AllocatedDays = lb.AllocatedDays,
                CarryOverDays = lb.CarryOverDays,
                ManualAdjustmentDays = lb.ManualAdjustmentDays,
                UsedDays = lb.UsedDays,
                AvailableDays = lb.AllocatedDays + lb.CarryOverDays + lb.ManualAdjustmentDays - lb.UsedDays
            })
            .ToListAsync(cancellationToken);

        var leaveBalanceSummary = new LeaveBalanceSummaryDto
        {
            TotalAllocatedDays = leaveBalances.Sum(lb => lb.AllocatedDays + lb.CarryOverDays + lb.ManualAdjustmentDays),
            TotalUsedDays = leaveBalances.Sum(lb => lb.UsedDays),
            TotalAvailableDays = leaveBalances.Sum(lb => lb.AvailableDays)
        };

        var leaveHistory = await _context.EmployeeLeaveTransactions
            .AsNoTracking()
            .Where(t => scopedEmployeeIds == null || scopedEmployeeIds.Contains(t.EmployeeId))
            .OrderByDescending(t => t.CreatedDate)
            .Take(safeLeaveHistoryCount)
            .Select(t => new LeaveHistoryDto
            {
                TransactionId = t.Id,
                EmployeeId = t.EmployeeId,
                EmployeeName = _context.Employees
                    .Where(e => e.Id == t.EmployeeId)
                    .Select(e => e.FullNameEn)
                    .FirstOrDefault() ?? string.Empty,
                VacationType = _context.VacationTypes
                    .Where(v => v.Id == t.VacationTypeId)
                    .Select(v => v.NameEn)
                    .FirstOrDefault() ?? string.Empty,
                Year = t.Year,
                TransactionType = t.TransactionType.ToString(),
                DaysChanged = t.DaysChanged,
                BalanceAfter = t.BalanceAfter,
                ReferenceType = t.ReferenceType,
                ReferenceId = t.ReferenceId,
                CreatedDate = t.CreatedDate
            })
            .ToListAsync(cancellationToken);

        var leaveRequestsQuery = _context.EmployeeRequests
            .AsNoTracking()
            .Where(r => r.RequestTypeRef != null)
            .Where(r => r.RequestTypeRef!.Code.ToLower() == "vacation")
            .Where(r => scopedEmployeeIds == null || scopedEmployeeIds.Contains(r.EmployeeId));

        var leaveRequestSummary = new LeaveRequestApprovalSummaryDto
        {
            TotalLeaveRequests = await leaveRequestsQuery.CountAsync(cancellationToken),
            PendingRequests = await leaveRequestsQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Pending, cancellationToken),
            ManagerApprovedRequests = await leaveRequestsQuery.CountAsync(r => r.Status == EmployeeRequestStatus.ManagerApproved, cancellationToken),
            ApprovedRequests = await leaveRequestsQuery.CountAsync(r =>
                r.Status == EmployeeRequestStatus.Approved ||
                r.Status == EmployeeRequestStatus.Completed,
                cancellationToken),
            RejectedRequests = await leaveRequestsQuery.CountAsync(r => r.Status == EmployeeRequestStatus.Rejected, cancellationToken)
        };

        var recentLeaveRequests = await leaveRequestsQuery
            .OrderByDescending(r => r.RequestedDate)
            .Take(safeLeaveRequestsCount)
            .Select(r => new LeaveRequestDto
            {
                RequestId = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee.FullNameEn,
                RequestType = r.RequestTypeRef != null ? r.RequestTypeRef.NameEn : string.Empty,
                Status = r.Status,
                RequestedDate = r.RequestedDate,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                ApprovedBy = r.ApprovedBy,
                ApprovedDate = r.ApprovedDate
            })
            .ToListAsync(cancellationToken);

        return new AttendanceAndLeaveDto
        {
            DailyAttendanceSummary = dailySummary,
            DailyAttendanceRecords = dailyRecords,
            LeaveBalanceSummary = leaveBalanceSummary,
            LeaveBalances = leaveBalances,
            LeaveHistory = leaveHistory,
            LeaveRequestSummary = leaveRequestSummary,
            RecentLeaveRequests = recentLeaveRequests
        };
    }
}
