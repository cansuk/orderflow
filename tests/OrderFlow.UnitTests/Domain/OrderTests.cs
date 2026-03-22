using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Events;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.UnitTests.Domain;

public class OrderTests
{
    private static List<OrderItem> CreateValidItems() =>
    [
        OrderItem.Create(Guid.NewGuid(), "Product A", 2, 10.00m),
        OrderItem.Create(Guid.NewGuid(), "Product B", 1, 25.00m)
    ];

    [Fact]
    public void Create_WithValidData_ShouldCreateOrder()
    {
        var customerId = Guid.NewGuid();
        var items = CreateValidItems();

        var order = Order.Create(customerId, items);

        order.Id.Should().NotBeEmpty();
        order.CustomerId.Should().Be(customerId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.TotalAmount.Should().Be(45.00m);
        order.Items.Should().HaveCount(2);
    }

    [Fact]
    public void Create_ShouldRaise_OrderCreatedEvent()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());

        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCreatedEvent>();
    }

    [Fact]
    public void Create_WithEmptyCustomerId_ShouldThrow()
    {
        var act = () => Order.Create(Guid.Empty, CreateValidItems());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNoItems_ShouldThrow()
    {
        var act = () => Order.Create(Guid.NewGuid(), new List<OrderItem>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Confirm_FromPending_ShouldSucceed()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.ClearDomainEvents();

        order.Confirm();

        order.Status.Should().Be(OrderStatus.Confirmed);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderConfirmedEvent>();
    }

    [Fact]
    public void Confirm_FromConfirmed_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();

        var act = () => order.Confirm();
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void StartProcessing_FromConfirmed_ShouldSucceed()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();

        order.StartProcessing();

        order.Status.Should().Be(OrderStatus.Processing);
    }

    [Fact]
    public void Ship_FromProcessing_ShouldSucceed()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();
        order.StartProcessing();
        order.ClearDomainEvents();

        order.Ship();

        order.Status.Should().Be(OrderStatus.Shipped);
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderShippedEvent>();
    }

    [Fact]
    public void Deliver_FromShipped_ShouldSucceed()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();
        order.StartProcessing();
        order.Ship();

        order.Deliver();

        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void Cancel_FromPending_ShouldSucceed()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.ClearDomainEvents();

        order.Cancel("Customer request");

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer request");
        order.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<OrderCancelledEvent>();
    }

    [Fact]
    public void Cancel_FromShipped_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();
        order.StartProcessing();
        order.Ship();

        var act = () => order.Cancel("Too late");
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void Cancel_FromDelivered_ShouldThrow()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.Confirm();
        order.StartProcessing();
        order.Ship();
        order.Deliver();

        var act = () => order.Cancel("Too late");
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var order = Order.Create(Guid.NewGuid(), CreateValidItems());
        order.DomainEvents.Should().NotBeEmpty();

        order.ClearDomainEvents();

        order.DomainEvents.Should().BeEmpty();
    }
}
