using FluentValidation;
using OrderFlow.Application.Commands.UpdateOrderStatus;

namespace OrderFlow.Application.Validators;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Invalid order status.");
    }
}
