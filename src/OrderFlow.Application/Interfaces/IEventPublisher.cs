using OrderFlow.Domain.Events;

namespace OrderFlow.Application.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync(DomainEvent domainEvent, CancellationToken cancellationToken = default);
}
