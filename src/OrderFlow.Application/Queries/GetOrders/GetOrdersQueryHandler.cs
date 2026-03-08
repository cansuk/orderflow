using MediatR;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;

namespace OrderFlow.Application.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(request.Page, request.PageSize, cancellationToken);
        var totalCount = await _orderRepository.GetTotalCountAsync(cancellationToken);

        var dtos = orders.Select(o => o.ToDto()).ToList();

        return new PagedResult<OrderDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
