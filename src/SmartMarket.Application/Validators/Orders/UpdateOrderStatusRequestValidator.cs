using FluentValidation;
using SmartMarket.Application.DTOs.Orders;

namespace SmartMarket.Application.Validators.Orders;

public sealed class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
