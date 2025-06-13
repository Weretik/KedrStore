namespace Application.Common.Abstractions.Commands
{
    /// <summary>
    /// Интерфейс для обработчиков команд
    /// </summary>
    /// <typeparam name="TCommand">Тип команды</typeparam>
    /// <typeparam name="TResult">Тип результата выполнения команды</typeparam>
    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Обрабатывает команду
        /// </summary>
        /// <param name="command">Команда для обработки</param>
        /// <param name="cancellationToken">Токен отмены</param>
        /// <returns>Результат выполнения команды</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Интерфейс для обработчиков команд без возвращаемого результата
    /// </summary>
    /// <typeparam name="TCommand">Тип команды</typeparam>
    public interface ICommandHandler<in TCommand> : ICommandHandler<TCommand, Unit>
        where TCommand : ICommand
    {
    }
}
