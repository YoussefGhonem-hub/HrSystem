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
            request.StatusId,
            request.IsLate,
            request.IsOvertime,
            request.SearchTerm);

        var isSingleDayRange = effectiveFromDate.HasValue &&
                               effectiveToDate.HasValue &&
                               effectiveFromDate.Value == effectiveToDate.Value;

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
                .Where(s => s.Id == AttendanceStatusIds.Absent || s.Id == AttendanceStatusIds.OnLeave)
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

                if (request.IsLate == true || request.IsOvertime == true)
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
}
