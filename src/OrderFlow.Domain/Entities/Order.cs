using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<DomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Order() { }

    public static Order Create(Guid customerId, IEnumerable<OrderItem> items)
    {
        if (customerId == Guid.Empty) throw new ArgumentException("Customer ID is required.", nameof(customerId));

        var itemList = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
        if (itemList.Count == 0) throw new ArgumentException("Order must have at least one item.", nameof(items));

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        order._items.AddRange(itemList);
        order.TotalAmount = itemList.Sum(i => i.TotalPrice);
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId, order.TotalAmount));

        return order;
    }

    public void Confirm()
    {
        EnsureStatus(OrderStatus.Pending, "confirm");
        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderConfirmedEvent(Id));
    }

    public void StartProcessing()
    {
        EnsureStatus(OrderStatus.Confirmed, "start processing");
        Status = OrderStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        EnsureStatus(OrderStatus.Processing, "ship");
        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderShippedEvent(Id));
    }

    public void Deliver()
    {
        EnsureStatus(OrderStatus.Shipped, "deliver");
        Status = OrderStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOrderStateException(Status.ToString(), "cancel");

        Status = OrderStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new OrderCancelledEvent(Id, reason));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    private void EnsureStatus(OrderStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidOrderStateException(Status.ToString(), action);
    }
}
