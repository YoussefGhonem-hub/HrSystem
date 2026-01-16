using ErrorOr;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;

public record GetJobTitleByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<JobTitleDto>>>;
