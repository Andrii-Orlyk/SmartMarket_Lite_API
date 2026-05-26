using FluentValidation;
using SmartMarket.Application.DTOs.Products;

namespace SmartMarket.Application.Validators.Products;

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.SKU)
            .NotEmpty()
            .MaximumLength(80);

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);
    }
}
