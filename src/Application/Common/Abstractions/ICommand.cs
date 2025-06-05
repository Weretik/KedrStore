namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для команд, которые выполняют действия в системе и изменяют состояние
    /// </summary>
    /// <typeparam name="TResult">Тип результата выполнения команды</typeparam>
    public interface ICommand<out TResult> : IUseCase
    {
    }

    /// <summary>
    /// Интерфейс для команд без возвращаемого результата
    /// </summary>
    public interface ICommand : ICommand<Unit>
    {
    }

    /// <summary>
    /// Пустой тип для представления void в Generic контекстах
    /// </summary>
    public readonly struct Unit
    {
        /// <summary>
        /// Единственный экземпляр структуры Unit
        /// </summary>
        public static readonly Unit Value = new();
    }
}
