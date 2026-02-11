using FluentValidation;
using ProductService.Api.Contracts;

namespace ProductService.Api.Validation;

public class GetProductRequestValidator : AbstractValidator<GetProductsRequest>
{
    public GetProductRequestValidator()
    {
        RuleFor(r =>  r.Name).MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(r => r.CreatedToDate).GreaterThanOrEqualTo(r=>r.CreatedFromDate)
            .When(x => x.CreatedFromDate.HasValue && x.CreatedToDate.HasValue).WithMessage("CreatedToDate must be greater than CreatedFromDate");
        RuleFor(r=>r.PriceTo).GreaterThanOrEqualTo(r=>r.PriceFrom)
            .When(x => x.PriceFrom.HasValue && x.PriceTo.HasValue).WithMessage("PriceTo must be greater than PriceFrom");
    }
}