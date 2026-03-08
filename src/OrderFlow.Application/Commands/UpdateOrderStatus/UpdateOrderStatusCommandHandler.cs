using MediatR;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Application.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher,
        ICacheService cacheService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
        _cacheService = cacheService;
    }

    public async Task Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed: order.Confirm(); break;
            case OrderStatus.Processing: order.StartProcessing(); break;
            case OrderStatus.Shipped: order.Ship(); break;
            case OrderStatus.Delivered: order.Deliver(); break;
            case OrderStatus.Cancelled: order.Cancel("Status updated via API"); break;
            default:
                throw new InvalidOrderStateException(order.Status.ToString(), $"transition to {request.NewStatus}");
        }

        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);

        order.ClearDomainEvents();
        await _cacheService.RemoveAsync($"order:{request.OrderId}", cancellationToken);
    }
}
