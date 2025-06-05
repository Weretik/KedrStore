using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{
    /// <summary>
    /// Универсальный интерфейс для обработчиков запросов в системе
    /// </summary>
    /// <typeparam name="TRequest">Тип запроса</typeparam>
    /// <typeparam name="TResponse">Тип ответа</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IUseCase
    {
        /// <summary>
        /// Асинхронно обрабатывает запрос
        /// </summary>
        /// <param name="request">Запрос для обработки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат обработки запроса</returns>
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
