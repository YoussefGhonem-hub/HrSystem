using ErrorOr;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.JobTitles.Commands.UpdateJobTitle;

public record UpdateJobTitleCommand(Guid Id, UpdateJobTitleDto JobTitle) : IRequest<ErrorOr<GenericResponse<JobTitleDto>>>;
