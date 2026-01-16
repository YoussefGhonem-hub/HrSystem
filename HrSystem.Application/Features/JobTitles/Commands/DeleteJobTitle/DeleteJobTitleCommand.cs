using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.JobTitles.Commands.DeleteJobTitle;

public record DeleteJobTitleCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
