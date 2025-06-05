using System;

namespace Application.Common.Abstractions.Events
{
    /// <summary>
    /// Определяет базовый интерфейс для всех доменных событий
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Уникальный идентификатор события
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Дата и время возникновения события
        /// </summary>
        DateTime OccurredOn { get; }
    }
}
