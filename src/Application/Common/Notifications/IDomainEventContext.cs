namespace Application.Common.Notifications;

public interface IDomainEventContext
{
    IReadOnlyList<IHasDomainEvents> GetDomainEntities();
    void ClearDomainEvents();
}
