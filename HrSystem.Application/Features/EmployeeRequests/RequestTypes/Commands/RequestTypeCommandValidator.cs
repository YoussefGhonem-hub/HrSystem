using FluentValidation;

namespace HrSystem.Application.Features.EmployeeRequests.RequestTypes.Commands;

public class CreateRequestTypeCommandValidator : AbstractValidator<CreateRequestTypeCommand>
{
    public CreateRequestTypeCommandValidator()
    {
        RuleFor(x => x.Payload.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Payload.NameAr)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload.SortOrder)
            .GreaterThanOrEqualTo(1);
    }
}

public class UpdateRequestTypeCommandValidator : AbstractValidator<UpdateRequestTypeCommand>
{
    public UpdateRequestTypeCommandValidator()
    {
        RuleFor(x => x.Payload.Id)
            .NotEmpty();

        RuleFor(x => x.Payload.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Payload.NameAr)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload.NameEn)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Payload.SortOrder)
            .GreaterThanOrEqualTo(1);
    }
}
