using ErrorOr;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Organizations.Commands.UpdateBranchWorkSchedule;

/// <summary>
/// Updates work schedules for a branch (org-work-schedule-tab).
/// Supports add, update, and delete operations.
/// </summary>
public record UpdateBranchWorkScheduleCommand(
    Guid OrganizationId,
    Guid BranchId,
    List<WorkScheduleUpdateInput> Schedules
) : IRequest<ErrorOr<GenericResponse<WorkScheduleUpdateDto>>>;

public record WorkScheduleUpdateInput(
    Guid? Id,
    ScheduleAction Action,
    string? Name,
    TimeSpan? StartTime,
    TimeSpan? EndTime,
    TimeSpan? BreakDuration,
    int? WorkingHoursPerDay,
    int? WorkingDaysPerWeek,
    TimeSpan? GracePeriodLate,
    TimeSpan? GracePeriodEarlyLeave,
    bool? IsSunday,
    bool? IsMonday,
    bool? IsTuesday,
    bool? IsWednesday,
    bool? IsThursday,
    bool? IsFriday,
    bool? IsSaturday,
    bool? IsDefault,
    string? TimeZone
);

public enum ScheduleAction
{
    Add = 1,
    Update = 2,
    Delete = 3
}

public record WorkScheduleUpdateDto
{
    public Guid OrganizationId { get; init; }
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public List<WorkScheduleDto> Schedules { get; init; } = new();
    public int Added { get; init; }
    public int Updated { get; init; }
    public int Deleted { get; init; }
}

public record WorkScheduleDto
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
}

public class UpdateBranchWorkScheduleCommandHandler
    : IRequestHandler<UpdateBranchWorkScheduleCommand, ErrorOr<GenericResponse<WorkScheduleUpdateDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateBranchWorkScheduleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<WorkScheduleUpdateDto>>> Handle(
        UpdateBranchWorkScheduleCommand request,
        CancellationToken cancellationToken)
    {
        // Verify organization and branch exist
        var branch = await _context.Branches
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == request.BranchId && 
                                      b.OrganizationId == request.OrganizationId && 
                                      !b.IsDeleted, cancellationToken);

        if (branch == null)
            return Error.NotFound("Branch.NotFound", "Branch not found or does not belong to this organization");

        // Get existing schedules
        var existingSchedules = await _context.BranchWorkSchedules
            .Where(s => s.BranchId == request.BranchId && !s.IsDeleted)
            .ToListAsync(cancellationToken);

        int added = 0, updated = 0, deleted = 0;
        var newSchedules = new List<BranchWorkSchedule>();

        foreach (var input in request.Schedules)
        {
            switch (input.Action)
            {
                case ScheduleAction.Add:
                    if (string.IsNullOrEmpty(input.Name) || !input.StartTime.HasValue || !input.EndTime.HasValue)
                        return Error.Validation("Schedule.MissingFields", "Name, StartTime, and EndTime are required for new schedules");

                    var newSchedule = new BranchWorkSchedule
                    {
                        Id = Guid.NewGuid(),
                        TenantId = branch.OrganizationId,
                        BranchId = branch.Id,
                        Name = input.Name,
                        StartTime = input.StartTime.Value,
                        EndTime = input.EndTime.Value,
                        BreakDuration = input.BreakDuration ?? new TimeSpan(1, 0, 0),
                        WorkingHoursPerDay = input.WorkingHoursPerDay ?? 8,
                        WorkingDaysPerWeek = input.WorkingDaysPerWeek ?? 5,
                        GracePeriodLate = input.GracePeriodLate ?? new TimeSpan(0, 15, 0),
                        GracePeriodEarlyLeave = input.GracePeriodEarlyLeave ?? new TimeSpan(0, 15, 0),
                        IsSunday = input.IsSunday ?? true,
                        IsMonday = input.IsMonday ?? true,
                        IsTuesday = input.IsTuesday ?? true,
                        IsWednesday = input.IsWednesday ?? true,
                        IsThursday = input.IsThursday ?? true,
                        IsFriday = input.IsFriday ?? false,
                        IsSaturday = input.IsSaturday ?? false,
                        IsDefault = input.IsDefault ?? false,
                        TimeZone = input.TimeZone ?? branch.TimeZone,
                        IsActive = true,
                        CreatedDate = DateTimeOffset.UtcNow,
                        CreatedBy = CurrentUser.Id
                    };

                    // If this is set as default, unset other defaults
                    if (newSchedule.IsDefault)
                    {
                        foreach (var s in existingSchedules)
                            s.IsDefault = false;
                        foreach (var s in newSchedules)
                            s.IsDefault = false;
                    }

                    newSchedules.Add(newSchedule);
                    added++;
                    break;

                case ScheduleAction.Update:
                    if (!input.Id.HasValue)
                        return Error.Validation("Schedule.IdRequired", "Schedule ID is required for updates");

                    var scheduleToUpdate = existingSchedules.FirstOrDefault(s => s.Id == input.Id.Value);
                    if (scheduleToUpdate == null)
                        return Error.NotFound("Schedule.NotFound", $"Schedule with ID '{input.Id}' not found");

                    if (input.Name != null) scheduleToUpdate.Name = input.Name;
                    if (input.StartTime.HasValue) scheduleToUpdate.StartTime = input.StartTime.Value;
                    if (input.EndTime.HasValue) scheduleToUpdate.EndTime = input.EndTime.Value;
                    if (input.BreakDuration.HasValue) scheduleToUpdate.BreakDuration = input.BreakDuration;
                    if (input.WorkingHoursPerDay.HasValue) scheduleToUpdate.WorkingHoursPerDay = input.WorkingHoursPerDay.Value;
                    if (input.WorkingDaysPerWeek.HasValue) scheduleToUpdate.WorkingDaysPerWeek = input.WorkingDaysPerWeek.Value;
                    if (input.GracePeriodLate.HasValue) scheduleToUpdate.GracePeriodLate = input.GracePeriodLate;
                    if (input.GracePeriodEarlyLeave.HasValue) scheduleToUpdate.GracePeriodEarlyLeave = input.GracePeriodEarlyLeave;
                    if (input.IsSunday.HasValue) scheduleToUpdate.IsSunday = input.IsSunday.Value;
                    if (input.IsMonday.HasValue) scheduleToUpdate.IsMonday = input.IsMonday.Value;
                    if (input.IsTuesday.HasValue) scheduleToUpdate.IsTuesday = input.IsTuesday.Value;
                    if (input.IsWednesday.HasValue) scheduleToUpdate.IsWednesday = input.IsWednesday.Value;
                    if (input.IsThursday.HasValue) scheduleToUpdate.IsThursday = input.IsThursday.Value;
                    if (input.IsFriday.HasValue) scheduleToUpdate.IsFriday = input.IsFriday.Value;
                    if (input.IsSaturday.HasValue) scheduleToUpdate.IsSaturday = input.IsSaturday.Value;
                    if (input.TimeZone != null) scheduleToUpdate.TimeZone = input.TimeZone;

                    // Handle default flag
                    if (input.IsDefault.HasValue && input.IsDefault.Value)
                    {
                        foreach (var s in existingSchedules.Where(s => s.Id != input.Id))
                            s.IsDefault = false;
                        foreach (var s in newSchedules)
                            s.IsDefault = false;
                        scheduleToUpdate.IsDefault = true;
                    }
                    else if (input.IsDefault.HasValue)
                    {
                        scheduleToUpdate.IsDefault = false;
                    }

                    scheduleToUpdate.ModifiedDate = DateTimeOffset.UtcNow;
                    scheduleToUpdate.ModifiedBy = CurrentUser.Id;
                    updated++;
                    break;

                case ScheduleAction.Delete:
                    if (!input.Id.HasValue)
                        return Error.Validation("Schedule.IdRequired", "Schedule ID is required for deletion");

                    var scheduleToDelete = existingSchedules.FirstOrDefault(s => s.Id == input.Id.Value);
                    if (scheduleToDelete == null)
                        return Error.NotFound("Schedule.NotFound", $"Schedule with ID '{input.Id}' not found");

                    scheduleToDelete.IsDeleted = true;
                    scheduleToDelete.DeletedDate = DateTimeOffset.UtcNow;
                    scheduleToDelete.DeletedBy = CurrentUser.Id;
                    scheduleToDelete.IsActive = false;
                    deleted++;
                    break;
            }
        }

        // Add new schedules
        if (newSchedules.Count > 0)
            await _context.BranchWorkSchedules.AddRangeAsync(newSchedules, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Get all active schedules for response
        var allSchedules = await _context.BranchWorkSchedules
            .Where(s => s.BranchId == request.BranchId && !s.IsDeleted)
            .OrderByDescending(s => s.IsDefault)
            .ThenBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var dto = new WorkScheduleUpdateDto
        {
            OrganizationId = branch.OrganizationId,
            BranchId = branch.Id,
            BranchName = branch.NameEn,
            Added = added,
            Updated = updated,
            Deleted = deleted,
            Schedules = allSchedules.Select(s => new WorkScheduleDto
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
                TimeZone = s.TimeZone
            }).ToList()
        };

        return new GenericResponse<WorkScheduleUpdateDto>
        {
            Success = true,
            Message = $"Work schedules updated: {added} added, {updated} updated, {deleted} deleted",
            Data = dto
        };
    }
}
