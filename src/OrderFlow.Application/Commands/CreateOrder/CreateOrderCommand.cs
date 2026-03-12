using MediatR;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    List<CreateOrderItemRequest> Items) : IRequest<OrderDto>;
