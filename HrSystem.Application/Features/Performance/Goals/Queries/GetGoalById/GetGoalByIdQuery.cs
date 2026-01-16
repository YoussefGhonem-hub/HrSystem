using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;

public record GetGoalByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<GoalDto>>>;
