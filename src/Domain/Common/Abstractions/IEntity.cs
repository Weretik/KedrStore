namespace Domain.Common.Abstractions;

public interface IEntity<out TId>
{
    TId Id { get; }
}
