using ErrorOr;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;

namespace HrSystem.Application.Features.Attendance.Commands.CreateWorkSchedule;

public record CreateWorkScheduleCommand(
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

public record WorkScheduleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public int WorkingHoursPerDay { get; set; }
    public int WorkingDaysPerWeek { get; set; }
    public TimeSpan? GracePeriodLate { get; set; }
    public TimeSpan? GracePeriodEarlyLeave { get; set; }
    public bool IsSaturday { get; set; }
    public bool IsSunday { get; set; }
    public bool IsMonday { get; set; }
    public bool IsTuesday { get; set; }
    public bool IsWednesday { get; set; }
    public bool IsThursday { get; set; }
    public bool IsFriday { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateWorkScheduleCommandHandler : IRequestHandler<CreateWorkScheduleCommand, ErrorOr<GenericResponse<WorkScheduleDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateWorkScheduleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<WorkScheduleDto>>> Handle(CreateWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        var workSchedule = new WorkSchedule
        {
            Name = request.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakDuration = request.BreakDuration,
            WorkingHoursPerDay = request.WorkingHoursPerDay,
            WorkingDaysPerWeek = request.WorkingDaysPerWeek,
            GracePeriodLate = request.GracePeriodLate,
            GracePeriodEarlyLeave = request.GracePeriodEarlyLeave,
            IsSaturday = request.IsSaturday,
            IsSunday = request.IsSunday,
            IsMonday = request.IsMonday,
            IsTuesday = request.IsTuesday,
            IsWednesday = request.IsWednesday,
            IsThursday = request.IsThursday,
            IsFriday = request.IsFriday,
            IsDefault = request.IsDefault,
            TenantId = orgId.Value
        };

        workSchedule.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        await _context.WorkSchedules.AddAsync(workSchedule, cancellationToken);
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
            Message = "Work schedule created successfully",
            Data = dto
        };
    }
}
