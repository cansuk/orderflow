using FluentValidation;
using OrderFlow.Application.Commands.ConfirmOrder;

namespace OrderFlow.Application.Validators;

public sealed class ConfirmOrderCommandValidator : AbstractValidator<ConfirmOrderCommand>
{
    public ConfirmOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
    }
}
