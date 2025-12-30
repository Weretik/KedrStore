namespace BuildingBlocks.Domain.Abstractions;

public interface IEntityId<T>
{
    T Value { get; }
}
