namespace Domain.Abstractions;

/// <summary>
/// Определяет интерфейс для сущностей, которые могут генерировать события домена.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Получает коллекцию неподтвержденных событий домена.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Добавляет событие домена в коллекцию неподтвержденных событий.
    /// </summary>
    /// <param name="domainEvent">Событие домена для добавления.</param>
    void AddDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Удаляет событие домена из коллекции неподтвержденных событий.
    /// </summary>
    /// <param name="domainEvent">Событие домена для удаления.</param>
    void RemoveDomainEvent(IDomainEvent domainEvent);

    /// <summary>
    /// Очищает все неподтвержденные события домена.
    /// </summary>
    void ClearDomainEvents();
}
