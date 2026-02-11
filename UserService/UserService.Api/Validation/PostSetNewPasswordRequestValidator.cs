using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PostSetNewPasswordRequestValidator : AbstractValidator<PostSetNewPasswordRequest>
{
    public PostSetNewPasswordRequestValidator()
    {
        RuleFor(u=> u.Token)
            .NotEmpty().WithMessage("Token is required")
            .MaximumLength(200).WithMessage("Token must be less than 200 characters");
        RuleFor(u=> u.NewPassword)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}