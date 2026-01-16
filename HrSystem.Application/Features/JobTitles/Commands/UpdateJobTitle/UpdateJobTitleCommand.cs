using ErrorOr;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Shared.Common;
using MediatR;

namespace HrSystem.Application.Features.JobTitles.Commands.UpdateJobTitle;

public record UpdateJobTitleCommand(
    Guid Id,
    string TitleAr,
    string TitleEn,
    string? Description,
    int Level,
    decimal MinSalary,
    decimal MaxSalary
) : IRequest<ErrorOr<GenericResponse<JobTitleDto>>>;
