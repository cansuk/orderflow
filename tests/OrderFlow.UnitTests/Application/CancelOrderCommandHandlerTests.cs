using FluentAssertions;
using Moq;
using OrderFlow.Application.Commands.CancelOrder;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.UnitTests.Application;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _handler = new CancelOrderCommandHandler(
            _repositoryMock.Object, _unitOfWorkMock.Object,
            _eventPublisherMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WithPendingOrder_ShouldCancel()
    {
        var order = Order.Create(Guid.NewGuid(), new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Product", 1, 10m)
        });
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new CancelOrderCommand(order.Id, "Customer request"), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancellationReason.Should().Be("Customer request");
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldThrowNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _handler.Handle(new CancelOrderCommand(Guid.NewGuid(), "reason"), CancellationToken.None);
        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}
