using Domain.Abstractions;

namespace Domain.Events;

public abstract class BaseDomainEvent : IDomainEvent
{

    protected BaseDomainEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }

    public DateTime OccurredOn { get; }
}
