using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions.Queries
{
    /// <summary>
    /// Интерфейс для обработчиков запросов
    /// </summary>
    /// <typeparam name="TQuery">Тип запроса</typeparam>
    /// <typeparam name="TResult">Тип результата запроса</typeparam>
    public interface IQueryHandler<in TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Обрабатывает запрос
        /// </summary>
        /// <param name="query">Запрос для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат запроса</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
    }
}
