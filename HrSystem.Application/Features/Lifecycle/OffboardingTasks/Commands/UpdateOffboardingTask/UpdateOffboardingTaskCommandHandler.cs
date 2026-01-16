using ErrorOr;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.UpdateOffboardingTask;

public class UpdateOffboardingTaskCommandHandler : IRequestHandler<UpdateOffboardingTaskCommand, ErrorOr<GenericResponse<OffboardingTaskDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOffboardingTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OffboardingTaskDto>>> Handle(
        UpdateOffboardingTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OffboardingTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Offboarding task not found");
        }

        task.TaskNameAr = request.TaskNameAr;
        task.TaskNameEn = request.TaskNameEn;
        task.DescriptionAr = request.DescriptionAr;
        task.DescriptionEn = request.DescriptionEn;
        task.Sequence = request.Sequence;
        task.DueDate = request.DueDate;
        task.AssignedTo = request.AssignedTo;
        task.Category = request.Category;
        task.Notes = request.Notes;

        if (request.IsCompleted && !task.IsCompleted)
        {
            task.IsCompleted = true;
            task.CompletionDate = DateTime.UtcNow;
        }
        else if (!request.IsCompleted && task.IsCompleted)
        {
            task.IsCompleted = false;
            task.CompletionDate = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedTask = await _context.OffboardingTasks
            .Include(t => t.Employee)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        var dto = new OffboardingTaskDto
        {
            Id = updatedTask.Id,
            EmployeeId = updatedTask.EmployeeId,
            EmployeeName = updatedTask.Employee?.FullNameEn,
            TaskNameAr = updatedTask.TaskNameAr,
            TaskNameEn = updatedTask.TaskNameEn,
            DescriptionAr = updatedTask.DescriptionAr,
            DescriptionEn = updatedTask.DescriptionEn,
            Sequence = updatedTask.Sequence,
            DueDate = updatedTask.DueDate,
            IsCompleted = updatedTask.IsCompleted,
            CompletionDate = updatedTask.CompletionDate,
            AssignedTo = updatedTask.AssignedTo,
            Category = updatedTask.Category,
            Notes = updatedTask.Notes
        };

        return new GenericResponse<OffboardingTaskDto>
        {
            Success = true,
            Message = "Offboarding task updated successfully",
            Data = dto
        };
    }
}
