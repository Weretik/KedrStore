namespace Application.Common.Abstractions
{
    /// <summary>
    /// Интерфейс для запросов, которые получают данные из системы без изменения состояния
    /// </summary>
    /// <typeparam name="TResult">Тип результата запроса</typeparam>
    public interface IQuery<out TResult> : IUseCase
    {
    }
}
