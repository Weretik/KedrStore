using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common.Abstractions.Events;

namespace Application.Interfaces
{
    /// <summary>
    /// Интерфейс для реализации паттерна Unit of Work
    /// </summary>
    /// <remarks>
    /// Этот интерфейс обеспечивает единый контекст транзакции для всех операций с базой данных,
    /// гарантируя атомарность операций и отправку доменных событий после успешного сохранения.
    /// </remarks>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Контекст базы данных приложения
        /// </summary>
        IApplicationDbContext DbContext { get; }

        /// <summary>
        /// Диспетчер доменных событий
        /// </summary>
        IDomainEventDispatcher EventDispatcher { get; }

        /// <summary>
        /// Начинает новую транзакцию
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task BeginTransactionAsync();

        /// <summary>
        /// Фиксирует транзакцию
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task CommitTransactionAsync();

        /// <summary>
        /// Откатывает транзакцию
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task RollbackTransactionAsync();

        /// <summary>
        /// Сохраняет все изменения, внесенные в этом Unit of Work
        /// </summary>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Количество затронутых записей</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет указанное действие в транзакции
        /// </summary>
        /// <typeparam name="TResult">Тип результата</typeparam>
        /// <param name="action">Действие для выполнения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат выполнения действия</returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);

        /// <summary>
        /// Выполняет указанное действие в транзакции
        /// </summary>
        /// <param name="action">Действие для выполнения</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default);
    }
}
