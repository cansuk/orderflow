using MediatR;
using OrderFlow.Application.DTOs;

namespace OrderFlow.Application.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;
