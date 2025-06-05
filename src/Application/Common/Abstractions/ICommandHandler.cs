using System.Threading;
using System.Threading.Tasks;

namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для обработчиков команд, которые изменяют состояние системы
    /// </summary>
    /// <typeparam name="TCommand">Тип команды</typeparam>
    /// <typeparam name="TResult">Тип результата выполнения команды</typeparam>
    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Асинхронно обрабатывает команду
        /// </summary>
        /// <param name="command">Команда для обработки</param>
        /// <param name="cancellationToken">Токен отмены операции</param>
        /// <returns>Результат выполнения команды</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Интерфейс для обработчиков команд без возвращаемого результата
    /// </summary>
    /// <typeparam name="TCommand">Тип команды</typeparam>
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand<Unit>
    {
    }
}
