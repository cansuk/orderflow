using FluentAssertions;
using Moq;
using OrderFlow.Application.Commands.CreateOrder;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Interfaces;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Events;

namespace OrderFlow.UnitTests.Application;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IEventPublisher> _eventPublisherMock = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(
            _repositoryMock.Object, _unitOfWorkMock.Object, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_AndReturnDto()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product A", 2, 10.00m)
        });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be(command.CustomerId);
        result.TotalAmount.Should().Be(20.00m);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldCallRepository_AddAsync()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product A", 1, 10.00m)
        });

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUnitOfWork_SaveChanges()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product A", 1, 10.00m)
        });

        await _handler.Handle(command, CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPublish_OrderCreatedEvent()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product A", 1, 10.00m)
        });

        await _handler.Handle(command, CancellationToken.None);

        _eventPublisherMock.Verify(
            e => e.PublishAsync(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
