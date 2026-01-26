using ErrorOr;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.AssignEmployeeWorkSchedule;

public record AssignEmployeeWorkScheduleCommand(
    Guid EmployeeId,
    Guid WorkScheduleId,
    DateTime EffectiveDate
) : IRequest<ErrorOr<GenericResponse<bool>>>;

public class AssignEmployeeWorkScheduleCommandHandler : IRequestHandler<AssignEmployeeWorkScheduleCommand, ErrorOr<GenericResponse<bool>>>
{
    private readonly ApplicationDbContext _context;

    public AssignEmployeeWorkScheduleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<bool>>> Handle(AssignEmployeeWorkScheduleCommand request, CancellationToken cancellationToken)
    {
        var orgId = CurrentUser.OrganizationId;
        if (!orgId.HasValue)
        {
            return Error.Unauthorized(description: "No organization context");
        }

        // Validate employee
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId && e.TenantId == orgId.Value, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound(code: "Employee.NotFound", description: "Employee not found");
        }

        // Validate work schedule
        var workSchedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(ws => ws.Id == request.WorkScheduleId && ws.TenantId == orgId.Value, cancellationToken);

        if (workSchedule == null)
        {
            return Error.NotFound(code: "WorkSchedule.NotFound", description: "Work schedule not found");
        }

        // Mark all current schedules as not current
        var currentSchedules = await _context.EmployeeWorkSchedules
            .Where(ews => ews.EmployeeId == request.EmployeeId && ews.IsCurrent)
            .ToListAsync(cancellationToken);

        foreach (var schedule in currentSchedules)
        {
            schedule.IsCurrent = false;
            schedule.EndDate = request.EffectiveDate.AddDays(-1);
            schedule.MarkAsModified(CurrentUser.Id ?? Guid.Empty);
        }

        // Create new assignment
        var employeeWorkSchedule = new EmployeeWorkSchedule
        {
            EmployeeId = request.EmployeeId,
            WorkScheduleId = request.WorkScheduleId,
            EffectiveDate = request.EffectiveDate,
            IsCurrent = true,
            TenantId = orgId.Value
        };

        employeeWorkSchedule.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

        await _context.EmployeeWorkSchedules.AddAsync(employeeWorkSchedule, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse<bool>
        {
            Success = true,
            Message = $"Work schedule '{workSchedule.Name}' assigned to employee successfully",
            Data = true
        };
    }
}
