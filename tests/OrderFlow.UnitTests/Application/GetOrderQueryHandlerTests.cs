using FluentAssertions;
using Moq;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Application.Queries.GetOrder;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.UnitTests.Application;

public class GetOrderQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly GetOrderQueryHandler _handler;

    public GetOrderQueryHandlerTests()
    {
        _handler = new GetOrderQueryHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_WithCachedOrder_ShouldReturnFromCache()
    {
        var orderId = Guid.NewGuid();
        var cachedDto = new OrderDto(orderId, Guid.NewGuid(), OrderStatus.Pending,
            10m, null, DateTime.UtcNow, DateTime.UtcNow, Array.Empty<OrderItemDto>());

        _cacheMock.Setup(c => c.GetAsync<OrderDto>($"order:{orderId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        var result = await _handler.Handle(new GetOrderQuery(orderId), CancellationToken.None);

        result.Should().Be(cachedDto);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithoutCache_ShouldFetchAndCache()
    {
        var order = Order.Create(Guid.NewGuid(), new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), "Product", 1, 10m)
        });

        _cacheMock.Setup(c => c.GetAsync<OrderDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var result = await _handler.Handle(new GetOrderQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<OrderDto>(),
            It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistingOrder_ShouldThrow()
    {
        _cacheMock.Setup(c => c.GetAsync<OrderDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var act = () => _handler.Handle(new GetOrderQuery(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<OrderNotFoundException>();
    }
}
