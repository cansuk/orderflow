using FluentAssertions;
using OrderFlow.Application.Commands.CancelOrder;
using OrderFlow.Application.Commands.ConfirmOrder;
using OrderFlow.Application.Commands.CreateOrder;
using OrderFlow.Application.DTOs;
using OrderFlow.Application.Validators;

namespace OrderFlow.UnitTests.Application;

public class ValidatorTests
{
    [Fact]
    public async Task CreateOrderValidator_WithValidData_ShouldPass()
    {
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product", 1, 10m)
        });

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateOrderValidator_WithEmptyCustomerId_ShouldFail()
    {
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(Guid.Empty, new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product", 1, 10m)
        });

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateOrderValidator_WithEmptyItems_ShouldFail()
    {
        var validator = new CreateOrderCommandValidator();
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>());

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CancelOrderValidator_WithEmptyReason_ShouldFail()
    {
        var validator = new CancelOrderCommandValidator();
        var command = new CancelOrderCommand(Guid.NewGuid(), "");

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmOrderValidator_WithEmptyId_ShouldFail()
    {
        var validator = new ConfirmOrderCommandValidator();
        var command = new ConfirmOrderCommand(Guid.Empty);

        var result = await validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }
}
