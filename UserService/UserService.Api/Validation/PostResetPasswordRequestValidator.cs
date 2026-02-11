using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PostResetPasswordRequestValidator : AbstractValidator<PostResetPasswordRequest>
{
    public PostResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is invalid").MaximumLength(100).WithMessage("Email must be less than 100 characters");
    }
}