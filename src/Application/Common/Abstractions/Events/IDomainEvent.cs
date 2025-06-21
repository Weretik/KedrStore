namespace Application.Common.Abstractions.Events
{
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
    }
}
