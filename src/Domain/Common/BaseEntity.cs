using System.Collections.Generic;
using Domain.Abstractions;

namespace Domain.Common;

/// <summary>
/// Базовый класс для всех сущностей домена.
/// Реализует интерфейс IEntity и определяет основное поведение сущностей.
/// </summary>
/// <typeparam name="TId">Тип идентификатора сущности.</typeparam>
public abstract class BaseEntity<TId> : IEntity<TId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Получает или устанавливает уникальный идентификатор сущности.
    /// </summary>
    public TId Id { get; protected set; } = default!;

    /// <summary>
    /// Получает коллекцию неподтвержденных событий домена.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Добавляет событие домена в коллекцию неподтвержденных событий.
    /// </summary>
    /// <param name="domainEvent">Событие домена для добавления.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Удаляет событие домена из коллекции неподтвержденных событий.
    /// </summary>
    /// <param name="domainEvent">Событие домена для удаления.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Очищает все неподтвержденные события домена.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Проверяет, равны ли два объекта сущности.
    /// </summary>
    /// <param name="obj">Объект для сравнения.</param>
    /// <returns>True, если объекты равны; иначе false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (EqualityComparer<TId>.Default.Equals(Id, default) ||
            EqualityComparer<TId>.Default.Equals(other.Id, default))
            return false;

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    /// <summary>
    /// Возвращает хеш-код сущности, основанный на её идентификаторе.
    /// </summary>
    /// <returns>Хеш-код сущности.</returns>
    public override int GetHashCode()
    {
        return (GetType().ToString() + Id).GetHashCode();
    }

    /// <summary>
    /// Определяет оператор равенства для сущностей.
    /// </summary>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Определяет оператор неравенства для сущностей.
    /// </summary>
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !(left == right);
    }
}
