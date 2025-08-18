namespace Application.Common.Interfaces;

public interface IDomainEventContext
{
    IReadOnlyList<IHasDomainEvents> GetDomainEntities();
    void ClearDomainEvents();
}
