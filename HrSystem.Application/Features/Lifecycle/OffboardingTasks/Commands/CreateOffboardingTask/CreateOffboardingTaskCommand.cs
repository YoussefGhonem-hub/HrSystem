using ErrorOr;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Commands.CreateOffboardingTask;

public record CreateOffboardingTaskCommand(
    Guid EmployeeId,
    string TaskNameAr,
    string TaskNameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int Sequence,
    DateTime DueDate,
    Guid? AssignedTo,
    string Category,
    string? Notes
) : IRequest<ErrorOr<GenericResponse<OffboardingTaskDto>>>;

public class CreateOffboardingTaskCommandHandler : IRequestHandler<CreateOffboardingTaskCommand, ErrorOr<GenericResponse<OffboardingTaskDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateOffboardingTaskCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OffboardingTaskDto>>> Handle(
        CreateOffboardingTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = new Domain.Entities.Lifecycle.OffboardingTask
        {
            EmployeeId = request.EmployeeId,
            TaskNameAr = request.TaskNameAr,
            TaskNameEn = request.TaskNameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            Sequence = request.Sequence,
            DueDate = request.DueDate,
            AssignedTo = request.AssignedTo,
            Category = request.Category,
            Notes = request.Notes,
            IsCompleted = false,
            TenantId = Guid.NewGuid()
        };

        _context.OffboardingTasks.Add(task);
        await _context.SaveChangesAsync(cancellationToken);

        var createdTask = await _context.OffboardingTasks
            .Include(t => t.Employee)
            .FirstAsync(t => t.Id == task.Id, cancellationToken);

        var dto = new OffboardingTaskDto
        {
            Id = createdTask.Id,
            EmployeeId = createdTask.EmployeeId,
            EmployeeName = createdTask.Employee?.FullNameEn,
            TaskNameAr = createdTask.TaskNameAr,
            TaskNameEn = createdTask.TaskNameEn,
            DescriptionAr = createdTask.DescriptionAr,
            DescriptionEn = createdTask.DescriptionEn,
            Sequence = createdTask.Sequence,
            DueDate = createdTask.DueDate,
            IsCompleted = createdTask.IsCompleted,
            CompletionDate = createdTask.CompletionDate,
            AssignedTo = createdTask.AssignedTo,
            Category = createdTask.Category,
            Notes = createdTask.Notes
        };

        return new GenericResponse<OffboardingTaskDto>
        {
            Success = true,
            Message = "Offboarding task created successfully",
            Data = dto
        };
    }
}
