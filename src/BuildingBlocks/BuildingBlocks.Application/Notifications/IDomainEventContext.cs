using BuildingBlocks.Domain.Abstractions.Events;

namespace BuildingBlocks.Application.Notifications;

public interface IDomainEventContext
{
    IReadOnlyList<IHasDomainEvents> GetDomainEntities();
    void ClearDomainEvents();
}
