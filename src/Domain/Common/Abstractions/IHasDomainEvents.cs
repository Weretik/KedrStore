namespace Domain.Common.Abstractions;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent @event);
    void ClearDomainEvents();
}
