using FluentValidation;
using ProductService.Api.Contracts;

namespace ProductService.Api.Validation;

public class PutProductRequestValidator : AbstractValidator<PutProductRequest>
{
    public PutProductRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.Description).MaximumLength(500).WithMessage("Description must not exceed 500 characters");
        RuleFor(r=>r.Price).NotEmpty().WithMessage("Price is required");
        RuleFor(r => r.IsAvailable).NotNull().WithMessage("IsAvailable is required");
    }
}