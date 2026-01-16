using ErrorOr;
using HrSystem.Application.Features.Performance.GoalStatuses.Common;
using HrSystem.Domain.Entities.Performance;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Performance.GoalStatuses.Commands.CreateGoalStatus;

public record CreateGoalStatusCommand(
    string NameAr,
    string NameEn,
    string? DescriptionAr,
    string? DescriptionEn,
    int DisplayOrder,
    bool IsActive
) : IRequest<ErrorOr<GenericResponse<GoalStatusDto>>>;

public class CreateGoalStatusCommandHandler : IRequestHandler<CreateGoalStatusCommand, ErrorOr<GenericResponse<GoalStatusDto>>>
{
    private readonly ApplicationDbContext _context;

    public CreateGoalStatusCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<GoalStatusDto>>> Handle(
        CreateGoalStatusCommand request,
        CancellationToken cancellationToken)
    {
        var goalStatus = new GoalStatus
        {
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            DescriptionAr = request.DescriptionAr,
            DescriptionEn = request.DescriptionEn,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _context.GoalStatuses.Add(goalStatus);
        await _context.SaveChangesAsync(cancellationToken);

        var createdGoalStatus = await _context.GoalStatuses
            .Include(gs => gs.Goals)
            .FirstAsync(gs => gs.Id == goalStatus.Id, cancellationToken);

        var dto = new GoalStatusDto
        {
            Id = createdGoalStatus.Id,
            NameAr = createdGoalStatus.NameAr,
            NameEn = createdGoalStatus.NameEn,
            DescriptionAr = createdGoalStatus.DescriptionAr,
            DescriptionEn = createdGoalStatus.DescriptionEn,
            DisplayOrder = createdGoalStatus.DisplayOrder,
            IsActive = createdGoalStatus.IsActive,
            GoalsCount = createdGoalStatus.Goals.Count,
            CreatedDate = createdGoalStatus.CreatedDate
        };

        return new GenericResponse<GoalStatusDto>
        {
            Success = true,
            Message = "Goal status created successfully",
            Data = dto
        };
    }
}
