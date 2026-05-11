using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;

public record GetAttendancesListQuery(
    Guid? EmployeeId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? StatusId = null,
    bool? IsLate = null,
    bool? IsOvertime = null,
    string? SearchTerm = null,
    string? SortBy = null,
    bool IsDescending = false,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>>;

public class GetAttendancesListQueryHandler : IRequestHandler<GetAttendancesListQuery, ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendancesListQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<PagedResult<AttendanceListDto>>>> Handle(
        GetAttendancesListQuery request,
        CancellationToken cancellationToken)
    {
        var effectiveFromDate = request.FromDate?.Date;
        var effectiveToDate = request.ToDate?.Date;

        if (!effectiveFromDate.HasValue && !effectiveToDate.HasValue)
        {
            var today = DateTime.UtcNow.Date;
            effectiveFromDate = today;
            effectiveToDate = today;
        }
        else
        {
            effectiveFromDate ??= effectiveToDate;
            effectiveToDate ??= effectiveFromDate;
        }

        if (effectiveFromDate.HasValue && effectiveToDate.HasValue && effectiveFromDate.Value > effectiveToDate.Value)
        {
            (effectiveFromDate, effectiveToDate) = (effectiveToDate, effectiveFromDate);
        }

        var isSingleDayRange = effectiveFromDate.HasValue &&
                               effectiveToDate.HasValue &&
                               effectiveFromDate.Value == effectiveToDate.Value;

        var query = _context.Attendances
            .AsNoTracking()
            .ApplyBranchScope()
            .AsQueryable();

        // For regular employee users: if no employeeId is specified, automatically filter by current employee
        // HR users (HR, HRManager, HRSpecialist) and Managers can see all employees
        var employeeIdFilter = request.EmployeeId;
        if (!employeeIdFilter.HasValue && CurrentUser.EmployeeId.HasValue)
        {
            // Check if user has HR or management roles
            var isHrUser = CurrentUser.Roles.Any(r => 
                r.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("HRManager", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("HR Manager", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("HRSpecialist", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("HR Specialist", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("DepartmentManager", StringComparison.OrdinalIgnoreCase) ||
                r.Equals("Department Manager", StringComparison.OrdinalIgnoreCase));

            // Only filter by employee ID for regular employees (not HR/managers)
            if (!isHrUser && !CurrentUser.IsSuperAdmin && !CurrentUser.IsOrganizationAdmin)
            {
                employeeIdFilter = CurrentUser.EmployeeId.Value;
            }
        }

        // Apply filters
        query = query.ApplyFilters(
            employeeIdFilter,
            effectiveFromDate,
            effectiveToDate,
            isSingleDayRange ? null : request.StatusId,
            isSingleDayRange ? null : request.IsLate,
            request.IsOvertime,
            request.SearchTerm);

        if (isSingleDayRange)
        {
            var targetDate = effectiveFromDate!.Value;

            var existingDtos = await query.Select(a => new AttendanceListDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
                EmployeeCode = a.Employee.EmployeeCode,
                JobTitle = a.Employee.JobTitle != null ? a.Employee.JobTitle.TitleEn : null,
                Department = a.Employee.Department != null ? a.Employee.Department.NameEn : null,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                StatusId = a.StatusId,
                StatusNameEn = a.Status.NameEn,
                StatusNameAr = a.Status.NameAr,
                WorkedHours = a.WorkedHours,
                HalfDayRule = a.HalfDayRule,
                IsLate = a.IsLate,
                IsEarlyLeave = a.IsEarlyLeave,
                IsOvertime = a.IsOvertime
            }).ToListAsync(cancellationToken);

            var existingEmployeeIds = existingDtos
                .Select(x => x.EmployeeId)
                .ToHashSet();

            var employeesQuery = _context.Employees
                .AsNoTracking()
                .ApplyBranchScope()
                .Where(e => !e.IsDeleted && (!e.HiringDate.HasValue || e.HiringDate.Value.Date <= targetDate));

            if (employeeIdFilter.HasValue)
            {
                employeesQuery = employeesQuery.Where(e => e.Id == employeeIdFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                employeesQuery = employeesQuery.Where(e =>
                    e.FirstNameEn.ToLower().Contains(term) ||
                    e.LastNameEn.ToLower().Contains(term) ||
                    e.FirstNameAr.ToLower().Contains(term) ||
                    e.LastNameAr.ToLower().Contains(term) ||
                    e.EmployeeCode.ToLower().Contains(term));
            }

            var candidateEmployees = await employeesQuery
                .Select(e => new
                {
                    e.Id,
                    e.BranchId,
                    EmployeeName = e.FirstNameEn + " " + e.LastNameEn,
                    e.EmployeeCode,
                    JobTitle = e.JobTitle != null ? e.JobTitle.TitleEn : null,
                    Department = e.Department != null ? e.Department.NameEn : null
                })
                .ToListAsync(cancellationToken);

            var vacationRequestTypeId = await _context.RequestTypes
                .AsNoTracking()
                .Where(rt => rt.Code == "Vacation")
                .Select(rt => rt.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var onLeaveEmployeeIds = vacationRequestTypeId == Guid.Empty
                ? new List<Guid>()
                : await _context.EmployeeRequests
                    .AsNoTracking()
                    .ApplyBranchScope()
                    .Where(r => !r.IsDeleted &&
                                r.RequestTypeId == vacationRequestTypeId &&
                                r.Status == EmployeeRequestStatus.Approved &&
                                r.StartDate.HasValue &&
                                r.EndDate.HasValue &&
                                r.StartDate.Value.Date <= targetDate &&
                                r.EndDate.Value.Date >= targetDate)
                    .Select(r => r.EmployeeId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

            var onLeaveSet = onLeaveEmployeeIds.ToHashSet();

            var statusLookup = await _context.AttendanceStatuses
                .AsNoTracking()
                .Where(s => s.Id == AttendanceStatusIds.Absent ||
                            s.Id == AttendanceStatusIds.OnLeave ||
                            s.Id == AttendanceStatusIds.Holiday ||
                            s.Id == AttendanceStatusIds.Weekend)
                .Select(s => new { s.Id, s.NameEn, s.NameAr })
                .ToDictionaryAsync(s => s.Id, cancellationToken);

            var absentStatus = statusLookup.TryGetValue(AttendanceStatusIds.Absent, out var absent)
                ? absent
                : new { Id = AttendanceStatusIds.Absent, NameEn = "Absent", NameAr = "غائب" };

            var onLeaveStatus = statusLookup.TryGetValue(AttendanceStatusIds.OnLeave, out var onLeave)
                ? onLeave
                : new { Id = AttendanceStatusIds.OnLeave, NameEn = "On Leave", NameAr = "في إجازة" };

            var supplementalDtos = new List<AttendanceListDto>();

            foreach (var employee in candidateEmployees)
            {
                if (existingEmployeeIds.Contains(employee.Id))
                {
                    continue;
                }

                var isOnLeave = onLeaveSet.Contains(employee.Id);
                var statusId = isOnLeave ? AttendanceStatusIds.OnLeave : AttendanceStatusIds.Absent;
                var statusNameEn = isOnLeave ? onLeaveStatus.NameEn : absentStatus.NameEn;
                var statusNameAr = isOnLeave ? onLeaveStatus.NameAr : absentStatus.NameAr;

                if (request.StatusId.HasValue && request.StatusId.Value != statusId)
                {
                    continue;
                }

                supplementalDtos.Add(new AttendanceListDto
                {
                    Id = Guid.Empty,
                    EmployeeId = employee.Id,
                    EmployeeName = employee.EmployeeName,
                    EmployeeCode = employee.EmployeeCode,
                    JobTitle = employee.JobTitle,
                    Department = employee.Department,
                    Date = targetDate,
                    CheckInTime = null,
                    CheckOutTime = null,
                    StatusId = statusId,
                    StatusNameEn = statusNameEn,
                    StatusNameAr = statusNameAr,
                    WorkedHours = null,
                    HalfDayRule = null,
                    IsLate = false,
                    IsEarlyLeave = false,
                    IsOvertime = false
                });
            }

            var combined = existingDtos
                .Concat(supplementalDtos)
                .ToList();

            await EnrichAttendanceStatusesAsync(combined, cancellationToken);

            combined = ApplyInMemoryStatusFilters(combined, request.StatusId, request.IsLate, request.IsOvertime);

            combined = ApplyInMemorySorting(combined, request.SortBy, request.IsDescending);

            var combinedCount = combined.Count;
            var pagedItems = combined
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var singleDayResult = new PagedResult<AttendanceListDto>
            {
                Items = pagedItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = combinedCount,
                TotalPages = (int)Math.Ceiling(combinedCount / (double)request.PageSize)
            };

            return new GenericResponse<PagedResult<AttendanceListDto>>
            {
                Success = true,
                Data = singleDayResult
            };
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = query.ApplySorting(request.SortBy, request.IsDescending);

        // Apply pagination
        query = query.ApplyPaging(request.PageNumber, request.PageSize);

        // Project to DTO directly — avoids Include INNER JOIN issues
        var dtos = await query.Select(a => new AttendanceListDto
        {
            Id = a.Id,
            EmployeeId = a.EmployeeId,
            EmployeeName = a.Employee.FirstNameEn + " " + a.Employee.LastNameEn,
            EmployeeCode = a.Employee.EmployeeCode,
            JobTitle = a.Employee.JobTitle != null ? a.Employee.JobTitle.TitleEn : null,
            Department = a.Employee.Department != null ? a.Employee.Department.NameEn : null,
            Date = a.Date,
            CheckInTime = a.CheckInTime,
            CheckOutTime = a.CheckOutTime,
            StatusId = a.StatusId,
            StatusNameEn = a.Status.NameEn,
            StatusNameAr = a.Status.NameAr,
            WorkedHours = a.WorkedHours,
            HalfDayRule = a.HalfDayRule,
            IsLate = a.IsLate,
            IsEarlyLeave = a.IsEarlyLeave,
            IsOvertime = a.IsOvertime
        }).ToListAsync(cancellationToken);

        await EnrichAttendanceStatusesAsync(dtos, cancellationToken);

        var pagedResult = new PagedResult<AttendanceListDto>
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return new GenericResponse<PagedResult<AttendanceListDto>>
        {
            Success = true,
            Data = pagedResult
        };
    }

    private static List<AttendanceListDto> ApplyInMemorySorting(
        List<AttendanceListDto> items,
        string? sortBy,
        bool isDescending)
    {
        return sortBy?.ToLower() switch
        {
            "date" => isDescending
                ? items.OrderByDescending(x => x.Date).ToList()
                : items.OrderBy(x => x.Date).ToList(),
            "employee" => isDescending
                ? items.OrderByDescending(x => x.EmployeeName).ToList()
                : items.OrderBy(x => x.EmployeeName).ToList(),
            "status" => isDescending
                ? items.OrderByDescending(x => x.StatusNameEn).ToList()
                : items.OrderBy(x => x.StatusNameEn).ToList(),
            "workedhours" => isDescending
                ? items.OrderByDescending(x => x.WorkedHours).ToList()
                : items.OrderBy(x => x.WorkedHours).ToList(),
            _ => items.OrderByDescending(x => x.Date).ThenBy(x => x.EmployeeName).ToList()
        };
    }

    private static List<AttendanceListDto> ApplyInMemoryStatusFilters(
        List<AttendanceListDto> items,
        Guid? statusId,
        bool? isLate,
        bool? isOvertime)
    {
        var filtered = items.AsEnumerable();

        if (statusId.HasValue)
        {
            filtered = filtered.Where(x => x.StatusId == statusId.Value);
        }

        if (isLate.HasValue)
        {
            filtered = filtered.Where(x => x.IsLate == isLate.Value);
        }

        if (isOvertime.HasValue)
        {
            filtered = filtered.Where(x => x.IsOvertime == isOvertime.Value);
        }

        return filtered.ToList();
    }

    private async Task EnrichAttendanceStatusesAsync(List<AttendanceListDto> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var employeeIds = items
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToList();

        var minDate = items.Min(x => x.Date).Date;
        var maxDate = items.Max(x => x.Date).Date;

        var employeeBranchMap = await _context.Employees
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(e => employeeIds.Contains(e.Id))
            .Select(e => new { e.Id, e.BranchId })
            .ToDictionaryAsync(e => e.Id, e => e.BranchId, cancellationToken);

        var branchIds = employeeBranchMap.Values.Distinct().ToList();

        var schedules = await _context.BranchWorkSchedules
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(s => branchIds.Contains(s.BranchId) && s.IsActive && !s.IsDeleted)
            .Select(s => new BranchScheduleInfo(
                s.BranchId,
                s.IsDefault,
                s.CreatedDate,
                s.IsSunday,
                s.IsMonday,
                s.IsTuesday,
                s.IsWednesday,
                s.IsThursday,
                s.IsFriday,
                s.IsSaturday))
            .ToListAsync(cancellationToken);

        var scheduleByBranch = schedules
            .GroupBy(s => s.BranchId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(s => s.IsDefault)
                    .ThenByDescending(s => s.CreatedDate)
                    .First());

        var holidays = await _context.BranchHolidays
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(h => branchIds.Contains(h.BranchId) &&
                        h.IsActive &&
                        !h.IsDeleted &&
                        ((!h.IsRecurring && h.Date.Date >= minDate && h.Date.Date <= maxDate) || h.IsRecurring))
            .Select(h => new BranchHolidayInfo(
                h.BranchId,
                h.Date.Date,
                h.IsRecurring,
                h.RecurringMonth,
                h.RecurringDay))
            .ToListAsync(cancellationToken);

        var holidayByBranch = holidays
            .GroupBy(h => h.BranchId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var approvedPermissions = await _context.EmployeeRequests
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(r => !r.IsDeleted &&
                        employeeIds.Contains(r.EmployeeId) &&
                        r.Status == EmployeeRequestStatus.Approved &&
                        r.RequestTypeRef != null &&
                        r.RequestTypeRef.Code == "Permission" &&
                        r.PermissionDetail != null &&
                        r.PermissionDetail.PermissionDate.Date >= minDate &&
                        r.PermissionDetail.PermissionDate.Date <= maxDate)
            .Select(r => new
            {
                r.EmployeeId,
                Date = r.PermissionDetail!.PermissionDate.Date,
                PermissionTypeName = r.PermissionDetail.PermissionType != null
                    ? r.PermissionDetail.PermissionType.NameEn
                    : string.Empty
            })
            .ToListAsync(cancellationToken);

        var permissionByEmployeeDate = approvedPermissions
            .Where(p => !string.IsNullOrWhiteSpace(p.PermissionTypeName))
            .GroupBy(p => (p.EmployeeId, p.Date))
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(x => GetPermissionPriority(x.PermissionTypeName))
                    .Select(x => x.PermissionTypeName)
                    .First());

        var weekendStatus = await _context.AttendanceStatuses
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(s => s.Id == AttendanceStatusIds.Weekend)
            .Select(s => new { s.NameEn, s.NameAr })
            .FirstOrDefaultAsync(cancellationToken);

        var holidayStatus = await _context.AttendanceStatuses
            .AsNoTracking()
            .ApplyBranchScope()
            .Where(s => s.Id == AttendanceStatusIds.Holiday)
            .Select(s => new { s.NameEn, s.NameAr })
            .FirstOrDefaultAsync(cancellationToken);

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (!employeeBranchMap.TryGetValue(item.EmployeeId, out var branchId) || !branchId.HasValue)
            {
                continue;
            }

            var resolvedBranchId = branchId.Value;

            var date = item.Date.Date;
            var isHoliday = IsHoliday(date, resolvedBranchId, holidayByBranch);
            if (isHoliday)
            {
                items[i] = item with
                {
                    StatusId = AttendanceStatusIds.Holiday,
                    StatusNameEn = "holiday",
                    StatusNameAr = holidayStatus?.NameAr ?? "عطلة",
                    IsLate = false,
                    IsEarlyLeave = false
                };
                continue;
            }

            var isWeekend = IsWeekend(date, resolvedBranchId, scheduleByBranch);
            if (isWeekend)
            {
                items[i] = item with
                {
                    StatusId = AttendanceStatusIds.Weekend,
                    StatusNameEn = "weekend",
                    StatusNameAr = weekendStatus?.NameAr ?? "عطلة أسبوعية",
                    IsLate = false,
                    IsEarlyLeave = false
                };
                continue;
            }

            if (!permissionByEmployeeDate.TryGetValue((item.EmployeeId, date), out var permissionTypeName))
            {
                continue;
            }

            if (!ShouldApplyPermissionOverride(item))
            {
                continue;
            }

            var normalized = permissionTypeName.Trim().ToLowerInvariant();

            if (normalized.Contains("field"))
            {
                items[i] = item with
                {
                    StatusId = AttendanceStatusIds.Present,
                    StatusNameEn = "field_visit",
                    StatusNameAr = "زيارة ميدانية",
                    IsLate = false,
                    IsEarlyLeave = false
                };
            }
            else if (normalized.Contains("late"))
            {
                items[i] = item with
                {
                    StatusId = AttendanceStatusIds.Late,
                    StatusNameEn = "late_arrival_approved",
                    StatusNameAr = "تأخير معتمد",
                    IsLate = false
                };
            }
            else if (normalized.Contains("early"))
            {
                items[i] = item with
                {
                    StatusId = AttendanceStatusIds.EarlyLeave,
                    StatusNameEn = "early_departure_approved",
                    StatusNameAr = "انصراف مبكر معتمد",
                    IsEarlyLeave = false
                };
            }
        }
    }

    private static bool IsHoliday(
        DateTime date,
        Guid branchId,
        IReadOnlyDictionary<Guid, List<BranchHolidayInfo>> holidayByBranch)
    {
        if (!holidayByBranch.TryGetValue(branchId, out var holidays))
        {
            return false;
        }

        foreach (var holiday in holidays)
        {
            if (!holiday.IsRecurring && holiday.Date == date)
            {
                return true;
            }

            if (holiday.IsRecurring &&
                holiday.RecurringMonth is int recurringMonth &&
                holiday.RecurringDay is int recurringDay &&
                recurringMonth == date.Month &&
                recurringDay == date.Day)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsWeekend(
        DateTime date,
        Guid branchId,
        IReadOnlyDictionary<Guid, BranchScheduleInfo> scheduleByBranch)
    {
        if (!scheduleByBranch.TryGetValue(branchId, out var schedule))
        {
            return date.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
        }

        return date.DayOfWeek switch
        {
            DayOfWeek.Sunday => !schedule.IsSunday,
            DayOfWeek.Monday => !schedule.IsMonday,
            DayOfWeek.Tuesday => !schedule.IsTuesday,
            DayOfWeek.Wednesday => !schedule.IsWednesday,
            DayOfWeek.Thursday => !schedule.IsThursday,
            DayOfWeek.Friday => !schedule.IsFriday,
            DayOfWeek.Saturday => !schedule.IsSaturday,
            _ => false
        };
    }

    private static bool ShouldApplyPermissionOverride(AttendanceListDto item)
    {
        if (item.StatusId == AttendanceStatusIds.Absent ||
            item.StatusId == AttendanceStatusIds.Late ||
            item.StatusId == AttendanceStatusIds.EarlyLeave)
        {
            return true;
        }

        return (!item.CheckInTime.HasValue && !item.CheckOutTime.HasValue) || item.IsLate || item.IsEarlyLeave;
    }

    private static int GetPermissionPriority(string permissionTypeName)
    {
        var normalized = permissionTypeName.Trim().ToLowerInvariant();

        if (normalized.Contains("field"))
        {
            return 3;
        }

        if (normalized.Contains("late"))
        {
            return 2;
        }

        if (normalized.Contains("early"))
        {
            return 1;
        }

        return 0;
    }

    private sealed record BranchScheduleInfo(
        Guid BranchId,
        bool IsDefault,
        DateTimeOffset CreatedDate,
        bool IsSunday,
        bool IsMonday,
        bool IsTuesday,
        bool IsWednesday,
        bool IsThursday,
        bool IsFriday,
        bool IsSaturday);

    private sealed record BranchHolidayInfo(
        Guid BranchId,
        DateTime Date,
        bool IsRecurring,
        int? RecurringMonth,
        int? RecurringDay);
}
