using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PostSetAccountAsConfirmedRequestValidator : AbstractValidator<PostSetAccountAsConfirmedRequest>
{
    public PostSetAccountAsConfirmedRequestValidator()
    {
        RuleFor(u=> u.Token)
            .NotEmpty().WithMessage("Token is required")
            .MaximumLength(200).WithMessage("Token must be less than 200 characters");
    }
}