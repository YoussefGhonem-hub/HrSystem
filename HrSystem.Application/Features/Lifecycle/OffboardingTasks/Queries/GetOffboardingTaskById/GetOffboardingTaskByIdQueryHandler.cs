using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;

public class GetOffboardingTaskByIdQueryHandler : IRequestHandler<GetOffboardingTaskByIdQuery, ErrorOr<GenericResponse<OffboardingTaskDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOffboardingTaskByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OffboardingTaskDto>>> Handle(
        GetOffboardingTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OffboardingTasks
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Offboarding task not found");
        }

        var dto = new OffboardingTaskDto
        {
            Id = task.Id,
            EmployeeId = task.EmployeeId,
            EmployeeName = task.Employee?.FullNameEn,
            TaskNameAr = task.TaskNameAr,
            TaskNameEn = task.TaskNameEn,
            DescriptionAr = task.DescriptionAr,
            DescriptionEn = task.DescriptionEn,
            Sequence = task.Sequence,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted,
            CompletionDate = task.CompletionDate,
            AssignedTo = task.AssignedTo,
            Category = task.Category,
            Notes = task.Notes
        };

        return new GenericResponse<OffboardingTaskDto>
        {
            Success = true,
            Data = dto
        };
    }
}
