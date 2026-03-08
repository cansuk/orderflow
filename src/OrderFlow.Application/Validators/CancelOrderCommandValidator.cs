using FluentValidation;
using OrderFlow.Application.Commands.CancelOrder;

namespace OrderFlow.Application.Validators;

public sealed class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("Cancellation reason is required.");
    }
}
