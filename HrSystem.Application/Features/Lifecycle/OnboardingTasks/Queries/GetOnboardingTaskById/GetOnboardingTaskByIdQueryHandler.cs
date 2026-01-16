using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;

public class GetOnboardingTaskByIdQueryHandler : IRequestHandler<GetOnboardingTaskByIdQuery, ErrorOr<GenericResponse<OnboardingTaskDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetOnboardingTaskByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<OnboardingTaskDto>>> Handle(
        GetOnboardingTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var task = await _context.OnboardingTasks
            .Include(t => t.Employee)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (task == null)
        {
            return Error.NotFound(description: "Onboarding task not found");
        }

        var dto = new OnboardingTaskDto
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

        return new GenericResponse<OnboardingTaskDto>
        {
            Success = true,
            Data = dto
        };
    }
}
