using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PostRoleRequestValidator : AbstractValidator<PostRoleRequest>
{
    public PostRoleRequestValidator()
    {
        RuleFor(r=>r.Name).NotEmpty().WithMessage("Name is required").MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}