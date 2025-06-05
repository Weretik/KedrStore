using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions.Common
{
    /// <summary>
    /// Универсальный интерфейс для обработчиков запросов и команд
    /// </summary>
    /// <typeparam name="TRequest">Тип запроса или команды</typeparam>
    /// <typeparam name="TResponse">Тип ответа</typeparam>
    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IUseCase
    {
        /// <summary>
        /// Обрабатывает запрос или команду
        /// </summary>
        /// <param name="request">Запрос или команда</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат обработки</returns>
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
