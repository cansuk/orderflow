using MediatR;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(Guid OrderId, OrderStatus NewStatus) : IRequest;
