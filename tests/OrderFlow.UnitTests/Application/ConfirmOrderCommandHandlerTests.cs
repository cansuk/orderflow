using FluentAssertions;
using Moq;
using OrderFlow.Application.Commands.ConfirmOrder;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.UnitTests.Application;

public class ConfirmOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        _handler = new ConfirmOrderCommandHandler(
            _repositoryMock.Object, _unitOfWorkMock.Object,
            _eventPublisherMock.Object, _cacheMock.Object);
    }

    private static Order CreatePendingOrder()
    {
        return Order.Create(Guid.NewGuid(), new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Product A", 1, 10.00m)
        });
    }

    [Fact]
    public async Task Handle_WithExistingOrder_ShouldConfirmOrder()
    {
        var order = CreatePendingOrder();
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        order.Status.Should().Be(OrderStatus.Confirmed);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldThrowNotFound()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _handler.Handle(new ConfirmOrderCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<OrderNotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldInvalidateCache()
    {
        var order = CreatePendingOrder();
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        await _handler.Handle(new ConfirmOrderCommand(order.Id), CancellationToken.None);

        _cacheMock.Verify(c => c.RemoveAsync($"order:{order.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }
}
