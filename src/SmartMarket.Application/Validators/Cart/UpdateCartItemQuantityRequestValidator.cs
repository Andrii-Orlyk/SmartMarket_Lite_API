using FluentValidation;
using SmartMarket.Application.DTOs.Cart;

namespace SmartMarket.Application.Validators.Cart;

public sealed class UpdateCartItemQuantityRequestValidator : AbstractValidator<UpdateCartItemQuantityRequest>
{
    public UpdateCartItemQuantityRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
