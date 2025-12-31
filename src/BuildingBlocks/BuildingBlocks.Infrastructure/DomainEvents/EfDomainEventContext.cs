using BuildingBlocks.Application.Notifications;
using BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public sealed class EfDomainEventContext(IEnumerable<DbContext> contexts) : IDomainEventContext
{
    public IReadOnlyList<IHasDomainEvents> GetDomainEntities()
        => contexts
            .SelectMany(db => db.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity))
            .ToList();

    public void ClearDomainEvents()
    {
        var entities = contexts
            .SelectMany(db => db.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(e => e.Entity.DomainEvents.Count > 0)
                .Select(e => e.Entity));

        foreach (var entity in entities)
            entity.ClearDomainEvents();
    }
}
