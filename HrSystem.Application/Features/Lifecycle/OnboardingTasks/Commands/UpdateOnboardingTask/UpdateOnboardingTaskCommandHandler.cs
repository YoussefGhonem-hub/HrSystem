using ErrorOr;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Commands.UpdateOnboardingTask;

public class UpdateOnboardingTaskCommandHandler : IRequestHandler<UpdateOnboardingTaskCommand, ErrorOr<GenericResponse<OnboardingTaskDto>>>
{
    private readonly ApplicationDbContext _context;

    public UpdateOnboardingTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OnboardingTaskDto>>> Handle(
        UpdateOnboardingTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OnboardingTasks
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Onboarding task not found");
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

        // Update completion status
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

        var updatedTask = await _context.OnboardingTasks
            .Include(t => t.Employee)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        var dto = new OnboardingTaskDto
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

        return new GenericResponse<OnboardingTaskDto>
        {
            Success = true,
            Message = "Onboarding task updated successfully",
            Data = dto
        };
    }
}
