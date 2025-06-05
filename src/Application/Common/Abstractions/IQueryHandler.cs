using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для обработчиков запросов, которые получают данные из системы
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса</typeparam>
    /// <typeparam name="TResult">Тип результата запроса</typeparam>
    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Асинхронно обрабатывает запрос
        /// </summary>
        /// <param name="query">Запрос для обработки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат выполнения запроса</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
