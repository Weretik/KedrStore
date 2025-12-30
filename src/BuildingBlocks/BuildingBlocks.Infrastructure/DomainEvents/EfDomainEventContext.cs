using Application.Common.Notifications;
using BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.DomainEvents;

public sealed class EfDomainEventContext(DbContext db) : IDomainEventContext
{
    public IReadOnlyList<IHasDomainEvents> GetDomainEntities()
        => db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

    public void ClearDomainEvents()
    {
        var entities = db.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity);

        foreach (var entity in entities)
            entity.ClearDomainEvents();
    }
}
