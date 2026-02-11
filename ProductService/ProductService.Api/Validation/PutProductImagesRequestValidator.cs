using FluentValidation;
using ProductService.Api.Contracts;

namespace ProductService.Api.Validation;

public class PutProductImagesRequestValidator : AbstractValidator<PutProductImagesRequest>
{
    public PutProductImagesRequestValidator()
    {
        RuleFor(i => i.ProductImages)
            .NotEmpty().WithMessage("Photos are required for uploading");
    }
}