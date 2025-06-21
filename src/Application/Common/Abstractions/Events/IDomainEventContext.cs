namespace Application.Common.Abstractions.Events;

public interface IDomainEventContext
{
    IEnumerable<IHasDomainEvents> GetDomainEntities();
    void ClearDomainEvents();
}
