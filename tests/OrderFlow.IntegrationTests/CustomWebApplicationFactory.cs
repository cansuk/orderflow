using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application.Interfaces;
using OrderFlow.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace OrderFlow.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OrderFlowDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            services.AddDbContext<OrderFlowDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString()));

            services.AddDistributedMemoryCache();
            services.AddSingleton<IEventPublisher, FakeEventPublisher>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
            db.Database.EnsureCreated();
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync() => await _postgres.DisposeAsync();
}

public class FakeEventPublisher : IEventPublisher
{
    public List<Domain.Events.DomainEvent> PublishedEvents { get; } = new();

    public Task PublishAsync(Domain.Events.DomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        PublishedEvents.Add(domainEvent);
        return Task.CompletedTask;
    }
}
