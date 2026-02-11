using FluentValidation;
using ProductService.Api.Contracts;

namespace ProductService.Api.Validation;

public class PostProductRequestValidator : AbstractValidator<PostProductRequest>
{
    public PostProductRequestValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required").MaximumLength(100)
            .WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.Description).MaximumLength(500).WithMessage("Description must not exceed 500 characters");
        RuleFor(r => r.Price).NotEmpty().WithMessage("Price is required").GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0").LessThan(999999999999)
            .WithMessage("Price is to high");
        RuleFor(r => r.IsAvailable).NotNull().WithMessage("IsAvailable is required");
    }
}