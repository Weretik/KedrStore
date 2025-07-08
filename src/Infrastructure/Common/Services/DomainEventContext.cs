namespace Infrastructure.Common.Services;

public sealed class DomainEventContext(
    IEnumerable<DbContext> dbContexts)
    : IDomainEventContext
{
    public IEnumerable<IHasDomainEvents> GetDomainEntities()
    {
        return dbContexts
            .SelectMany(db => db.ChangeTracker
                .Entries<IHasDomainEvents>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity));
    }

    public void ClearDomainEvents()
    {
        foreach (var entity in GetDomainEntities())
            entity.ClearDomainEvents();
    }
}
