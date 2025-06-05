using System.Collections.Generic;
using Application.Common.Abstractions.Events;

namespace Application.Common.Abstractions.Entities
{
    /// <summary>
    /// Интерфейс для сущностей, которые могут содержать доменные события
    /// </summary>
    public interface IHasDomainEvents
    {
        /// <summary>
        /// Коллекция доменных событий, связанных с сущностью
        /// </summary>
        IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

        /// <summary>
        /// Добавляет доменное событие в коллекцию событий сущности
        /// </summary>
        /// <param name="domainEvent">Доменное событие</param>
        void AddDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// Удаляет доменное событие из коллекции событий сущности
        /// </summary>
        /// <param name="domainEvent">Доменное событие</param>
        void RemoveDomainEvent(IDomainEvent domainEvent);

        /// <summary>
        /// Очищает коллекцию доменных событий сущности
        /// </summary>
        void ClearDomainEvents();
    }
}
