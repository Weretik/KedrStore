namespace Domain.Common.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
