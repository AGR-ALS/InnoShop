using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PutUserRequestValidator : AbstractValidator<PutUserRequest>
{
    public PutUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100).WithMessage("Name must be less than 100 characters");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is invalid").MaximumLength(100).WithMessage("Email must be less than 100 characters");
        RuleFor(x=>x.Role).NotEmpty().WithMessage("Role is required").MaximumLength(50).WithMessage("Role must be less than 50 characters"); 
    }
}