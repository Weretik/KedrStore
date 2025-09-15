namespace Domain.Common.Abstractions;

public interface IEntityId<T>
{
    T Value { get; }
}
