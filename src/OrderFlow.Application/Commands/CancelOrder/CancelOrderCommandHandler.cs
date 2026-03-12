using MediatR;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Exceptions;

namespace OrderFlow.Application.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;
    private readonly ICacheService _cacheService;

    public CancelOrderCommandHandler(
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

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new OrderNotFoundException(request.OrderId);

        order.Cancel(request.Reason);
        _orderRepository.Update(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var domainEvent in order.DomainEvents)
            await _eventPublisher.PublishAsync(domainEvent, cancellationToken);

        order.ClearDomainEvents();
        await _cacheService.RemoveAsync($"order:{request.OrderId}", cancellationToken);
    }
}
