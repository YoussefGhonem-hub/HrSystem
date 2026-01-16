using ErrorOr;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestoneById;

public record GetGoalMilestoneByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<GoalMilestoneDto>>>;
