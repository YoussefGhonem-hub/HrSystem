using ErrorOr;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand(Guid Id, UpdateBranchDto Branch) : IRequest<ErrorOr<GenericResponse<BranchDto>>>;
