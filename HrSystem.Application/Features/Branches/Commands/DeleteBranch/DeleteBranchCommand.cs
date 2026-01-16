using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
