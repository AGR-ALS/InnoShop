using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class LoginUserRequestValidator :AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        RuleFor(u=> u.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address")
            .MaximumLength(100).WithMessage("Email must be less than 100 characters");
        RuleFor(u=> u.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}