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

        var branchId = CurrentUser.BranchId;
        // Base query scoped to date and optional branch
        var baseQuery = _context.Attendances
            .Include(a => a.Employee)
            .Where(a => a.Date.Date == date)
            .AsQueryable();

        if (branchId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.Employee.BranchId == branchId);
        }

        var totalPresent = await baseQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Present, cancellationToken);
        var lateToday = await baseQuery.CountAsync(a => a.IsLate, cancellationToken);
        var absentToday = await baseQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.Absent, cancellationToken);
        var onLeaveToday = await baseQuery.CountAsync(a => a.StatusId == AttendanceStatusIds.OnLeave, cancellationToken);

        var dto = new AttendanceDashboardDto
        {
            TotalPresent = totalPresent,
            LateArrivalToday = lateToday,
            AbsentToday = absentToday,
            OnLeaveToday = onLeaveToday,
            Date = date
        };

        return new GenericResponse<AttendanceDashboardDto>
        {
            Success = true,
            Message = "Attendance dashboard retrieved successfully",
            Data = dto
        };
    }
}
