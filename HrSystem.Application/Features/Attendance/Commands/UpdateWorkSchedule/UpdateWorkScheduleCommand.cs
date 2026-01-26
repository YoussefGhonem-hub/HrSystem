using ErrorOr;
using HrSystem.Application.Features.Attendance.Commands.CreateWorkSchedule;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.UpdateWorkSchedule;

public record UpdateWorkScheduleCommand(
    Guid Id,
    string Name,
    TimeSpan StartTime,
    TimeSpan EndTime,
    TimeSpan? BreakDuration,
    int WorkingHoursPerDay,
    int WorkingDaysPerWeek,
    TimeSpan? GracePeriodLate,
    TimeSpan? GracePeriodEarlyLeave,
    bool IsSaturday,
    bool IsSunday,
    bool IsMonday,
    bool IsTuesday,
    bool IsWednesday,
    bool IsThursday,
    bool IsFriday,
    bool IsDefault
) : IRequest<ErrorOr<GenericResponse<WorkScheduleDto>>>;

public class UpdateWorkScheduleCommandHandler : IRequestHandler<UpdateWorkScheduleCommand, ErrorOr<GenericResponse<WorkScheduleDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateWorkScheduleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<WorkScheduleDto>>> Handle(UpdateWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var workSchedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(ws => ws.Id == request.Id && ws.TenantId == orgId.Value, cancellationToken);

        if (workSchedule == null)
        {
            return Error.NotFound(code: "WorkSchedule.NotFound", description: "Work schedule not found");
        }

        workSchedule.Name = request.Name;
        workSchedule.StartTime = request.StartTime;
        workSchedule.EndTime = request.EndTime;
        workSchedule.BreakDuration = request.BreakDuration;
        workSchedule.WorkingHoursPerDay = request.WorkingHoursPerDay;
        workSchedule.WorkingDaysPerWeek = request.WorkingDaysPerWeek;
        workSchedule.GracePeriodLate = request.GracePeriodLate;
        workSchedule.GracePeriodEarlyLeave = request.GracePeriodEarlyLeave;
        workSchedule.IsSaturday = request.IsSaturday;
        workSchedule.IsSunday = request.IsSunday;
        workSchedule.IsMonday = request.IsMonday;
        workSchedule.IsTuesday = request.IsTuesday;
        workSchedule.IsWednesday = request.IsWednesday;
        workSchedule.IsThursday = request.IsThursday;
        workSchedule.IsFriday = request.IsFriday;
        workSchedule.IsDefault = request.IsDefault;

        workSchedule.MarkAsModified(CurrentUser.Id ?? Guid.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        var dto = new WorkScheduleDto
        {
            Id = workSchedule.Id,
            Name = workSchedule.Name,
            StartTime = workSchedule.StartTime,
            EndTime = workSchedule.EndTime,
            BreakDuration = workSchedule.BreakDuration,
            WorkingHoursPerDay = workSchedule.WorkingHoursPerDay,
            WorkingDaysPerWeek = workSchedule.WorkingDaysPerWeek,
            GracePeriodLate = workSchedule.GracePeriodLate,
            GracePeriodEarlyLeave = workSchedule.GracePeriodEarlyLeave,
            IsSaturday = workSchedule.IsSaturday,
            IsSunday = workSchedule.IsSunday,
            IsMonday = workSchedule.IsMonday,
            IsTuesday = workSchedule.IsTuesday,
            IsWednesday = workSchedule.IsWednesday,
            IsThursday = workSchedule.IsThursday,
            IsFriday = workSchedule.IsFriday,
            IsDefault = workSchedule.IsDefault
        };

        return new GenericResponse<WorkScheduleDto>
        {
            Success = true,
            Message = "Work schedule updated successfully",
            Data = dto
        };
    }
}
