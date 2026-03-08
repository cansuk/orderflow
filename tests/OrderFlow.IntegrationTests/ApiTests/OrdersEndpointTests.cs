using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Application.Commands.CreateOrder;
using OrderFlow.Application.DTOs;

namespace OrderFlow.IntegrationTests.ApiTests;

public class OrdersEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public OrdersEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", GenerateTestToken());
    }

    private static string GenerateTestToken()
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("super-secret-key-for-development-only-min-32-chars"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "OrderFlow",
            audience: "OrderFlow",
            claims: new[] { new Claim(ClaimTypes.Name, "test-user") },
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldReturn201()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Test Product", 2, 25.00m)
        });

        var response = await _client.PostAsJsonAsync("/api/v1/orders", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
        order.Should().NotBeNull();
        order!.TotalAmount.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetOrders_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/orders");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetOrder_WithNonExistingId_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndConfirmOrder_ShouldReturn204()
    {
        var createCommand = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product", 1, 10.00m)
        });
        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", createCommand);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var confirmResponse = await _client.PostAsync($"/api/v1/orders/{created!.Id}/confirm", null);
        confirmResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateAndCancelOrder_ShouldReturn204()
    {
        var createCommand = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product", 1, 10.00m)
        });
        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", createCommand);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var cancelResponse = await _client.PostAsJsonAsync(
            $"/api/v1/orders/{created!.Id}/cancel", new { Reason = "Test cancel" });
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetOrderById_AfterCreate_ShouldReturnOrder()
    {
        var createCommand = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>
        {
            new(Guid.NewGuid(), "Product", 1, 15.00m)
        });
        var createResponse = await _client.PostAsJsonAsync("/api/v1/orders", createCommand);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderDto>();

        var getResponse = await _client.GetAsync($"/api/v1/orders/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
        fetched!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task Unauthorized_WithoutToken_ShouldReturn401()
    {
        var client = new CustomWebApplicationFactory().CreateClient();
        var response = await client.GetAsync("/api/v1/orders");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateOrder_WithEmptyItems_ShouldReturn400()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), new List<CreateOrderItemRequest>());
        var response = await _client.PostAsJsonAsync("/api/v1/orders", command);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
