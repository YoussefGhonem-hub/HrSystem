using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceDashboard;

public record GetAttendanceDashboardQuery(DateTime? Date = null) : IRequest<ErrorOr<GenericResponse<AttendanceDashboardDto>>>;

public class GetAttendanceDashboardQueryHandler : IRequestHandler<GetAttendanceDashboardQuery, ErrorOr<GenericResponse<AttendanceDashboardDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetAttendanceDashboardQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<AttendanceDashboardDto>>> Handle(GetAttendanceDashboardQuery request, CancellationToken cancellationToken)
    {
        var date = request.Date?.Date ?? DateTime.UtcNow.Date;
        var yesterday = date.AddDays(-1);
        var sameDayLastWeek = date.AddDays(-7);

        var branchId = CurrentUser.BranchId;

        // Helper to scope a query by date + branch
        IQueryable<HrSystem.Domain.Entities.Attendance.Attendance> BuildQuery(DateTime targetDate)
        {
            var q = _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.Date.Date == targetDate)
                .AsQueryable();

            if (branchId.HasValue)
                q = q.Where(a => a.Employee.BranchId == branchId);

            return q;
        }

        // --- Today ---
        var todayQuery = BuildQuery(date);
        var totalPresent = await todayQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Present, cancellationToken);
        var lateToday = await todayQuery.CountAsync(a => a.IsLate, cancellationToken);
        var onLeaveToday = await todayQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.OnLeave, cancellationToken);

        // Count employees with an explicit Absent record
        var explicitAbsent = await todayQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Absent, cancellationToken);

        // Count active employees who have NO attendance record at all today — they are effectively absent
        var activeEmployeesQuery = _context.Employees
            .Where(e => !e.IsDeleted && e.StatusId == EmployeeStatusIds.Active);
        if (branchId.HasValue)
            activeEmployeesQuery = activeEmployeesQuery.Where(e => e.BranchId == branchId);
        var totalActive = await activeEmployeesQuery.CountAsync(cancellationToken);

        var employeesWithRecordToday = await _context.Attendances
            .Where(a => a.Date.Date == date && (!branchId.HasValue || a.Employee.BranchId == branchId))
            .Select(a => a.EmployeeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var absentToday = explicitAbsent + Math.Max(0, totalActive - employeesWithRecordToday);

        // --- Yesterday (for present & on-leave percentage change) ---
        var yesterdayQuery = BuildQuery(yesterday);
        var presentYesterday = await yesterdayQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Present, cancellationToken);
        var onLeaveYesterday = await yesterdayQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.OnLeave, cancellationToken);

        // --- Same day last week (for late & absent absolute change) ---
        var lastWeekQuery = BuildQuery(sameDayLastWeek);
        var lateLastWeek = await lastWeekQuery.CountAsync(a => a.IsLate, cancellationToken);
        var absentLastWeek = await lastWeekQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Absent, cancellationToken);

        // Calculate comparison values
        double presentChangePercent = presentYesterday > 0
            ? Math.Round((double)(totalPresent - presentYesterday) / presentYesterday * 100, 1)
            : 0;

        int lateChange = lateToday - lateLastWeek;
        int absentChange = absentToday - absentLastWeek;

        double onLeaveChangePercent = onLeaveYesterday > 0
            ? Math.Round((double)(onLeaveToday - onLeaveYesterday) / onLeaveYesterday * 100, 1)
            : 0;

        var dto = new AttendanceDashboardDto
        {
            Date = date,
            TotalPresent = totalPresent,
            TotalPresentChangePercent = presentChangePercent,
            LateArrivalToday = lateToday,
            LateArrivalChangeFromLastWeek = lateChange,
            AbsentToday = absentToday,
            AbsentChangeFromLastWeek = absentChange,
            OnLeaveToday = onLeaveToday,
            OnLeaveChangePercent = onLeaveChangePercent
        };

        return new GenericResponse<AttendanceDashboardDto>
        {
            Success = true,
            Message = "Attendance dashboard retrieved successfully",
            Data = dto
        };
    }
}
