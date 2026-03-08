using MediatR;

namespace OrderFlow.Application.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid OrderId) : IRequest;
