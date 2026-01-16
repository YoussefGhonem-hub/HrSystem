using FluentValidation;

namespace HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;

public class GetJobTitleByIdQueryValidator : AbstractValidator<GetJobTitleByIdQuery>
{
    public GetJobTitleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Job title ID is required");
    }
}
