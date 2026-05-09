using ErrorOr;
using HrSystem.Application.Common.Extensions;
using HrSystem.Application.Common.PaginatedList;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
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
            request.FromDate,
            request.ToDate,
            request.StatusId,
            request.IsLate,
            request.IsOvertime,
            request.SearchTerm);

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
}
