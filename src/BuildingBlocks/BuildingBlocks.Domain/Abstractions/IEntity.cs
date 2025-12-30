namespace BuildingBlocks.Domain.Abstractions;

public interface IEntity<out TId>
{
    TId Id { get; }
}
