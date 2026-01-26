using ErrorOr;
using HrSystem.Application.Features.Attendance.Commands.CreateWorkSchedule;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Queries.GetWorkSchedules;

public record GetWorkSchedulesQuery : IRequest<ErrorOr<GenericResponse<List<WorkScheduleDto>>>>;

public class GetWorkSchedulesQueryHandler : IRequestHandler<GetWorkSchedulesQuery, ErrorOr<GenericResponse<List<WorkScheduleDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetWorkSchedulesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<WorkScheduleDto>>>> Handle(GetWorkSchedulesQuery request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var schedules = await _context.WorkSchedules
            .Where(ws => ws.TenantId == orgId.Value)
            .OrderByDescending(ws => ws.IsDefault)
            .ThenBy(ws => ws.Name)
            .Select(ws => new WorkScheduleDto
            {
                Id = ws.Id,
                Name = ws.Name,
                StartTime = ws.StartTime,
                EndTime = ws.EndTime,
                BreakDuration = ws.BreakDuration,
                WorkingHoursPerDay = ws.WorkingHoursPerDay,
                WorkingDaysPerWeek = ws.WorkingDaysPerWeek,
                GracePeriodLate = ws.GracePeriodLate,
                GracePeriodEarlyLeave = ws.GracePeriodEarlyLeave,
                IsSaturday = ws.IsSaturday,
                IsSunday = ws.IsSunday,
                IsMonday = ws.IsMonday,
                IsTuesday = ws.IsTuesday,
                IsWednesday = ws.IsWednesday,
                IsThursday = ws.IsThursday,
                IsFriday = ws.IsFriday,
                IsDefault = ws.IsDefault
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<WorkScheduleDto>>
        {
            Success = true,
            Message = "Work schedules retrieved successfully",
            Data = schedules
        };
    }
}
