using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<BranchDto>>>;
