using MediatR;

namespace OrderFlow.Domain.Events;

public abstract record DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record OrderCreatedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount) : DomainEvent;

public sealed record OrderConfirmedEvent(Guid OrderId) : DomainEvent;

public sealed record OrderCancelledEvent(Guid OrderId, string Reason) : DomainEvent;

public sealed record OrderShippedEvent(Guid OrderId) : DomainEvent;
