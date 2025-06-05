using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions.Events
{
    /// <summary>
    /// Интерфейс для обработчиков доменных событий
    /// </summary>
    /// <typeparam name="TEvent">Тип обрабатываемого события</typeparam>
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        /// <summary>
        /// Обрабатывает доменное событие
        /// </summary>
        /// <param name="domainEvent">Доменное событие</param>
        /// <param name="cancellationToken">Токен отмены</param>
        Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
