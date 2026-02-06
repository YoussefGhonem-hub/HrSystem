using FluentValidation;

namespace HrSystem.Application.Features.Users.Commands.UpdateAccountSettings;

public class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.UserName)
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("Username can only contain letters, numbers, dots, and underscores")
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.WorkEmail)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.WorkEmail));

        RuleFor(x => x.AccountStatus)
            .Must(status => status == null || 
                           status.Equals("Active", StringComparison.OrdinalIgnoreCase) || 
                           status.Equals("Inactive", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Account status must be 'Active' or 'Inactive'");
    }
}
