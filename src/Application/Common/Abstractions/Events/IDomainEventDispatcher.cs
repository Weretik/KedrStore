using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions.Events
{
    /// <summary>
    /// Интерфейс для диспетчера доменных событий
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Отправляет доменное событие обработчикам
        /// </summary>
        /// <param name="domainEvent">Доменное событие</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отправляет коллекцию доменных событий обработчикам
        /// </summary>
        /// <param name="domainEvents">Коллекция доменных событий</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
    }
}
