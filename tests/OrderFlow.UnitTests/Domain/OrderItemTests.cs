using FluentAssertions;
using OrderFlow.Domain.ValueObjects;

namespace OrderFlow.UnitTests.Domain;

public class OrderItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateItem()
    {
        var productId = Guid.NewGuid();
        var item = OrderItem.Create(productId, "Test Product", 3, 15.50m);

        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be("Test Product");
        item.Quantity.Should().Be(3);
        item.UnitPrice.Should().Be(15.50m);
        item.TotalPrice.Should().Be(46.50m);
    }

    [Fact]
    public void Create_WithEmptyProductId_ShouldThrow()
    {
        var act = () => OrderItem.Create(Guid.Empty, "Test", 1, 10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "", 1, 10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrow()
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Test", 0, 10m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrow()
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), "Test", 1, -5m);
        act.Should().Throw<ArgumentException>();
    }
}
