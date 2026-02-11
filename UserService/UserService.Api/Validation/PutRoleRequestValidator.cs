using FluentValidation;
using UserService.Api.Contracts;

namespace UserService.Api.Validation;

public class PutRoleRequestValidator : AbstractValidator<PutRoleRequest>
{
    public PutRoleRequestValidator()
    {
        RuleFor(r=>r.Name).NotEmpty().WithMessage("Name is required").MaximumLength(50).WithMessage("Name must not exceed 50 characters");
    }
}