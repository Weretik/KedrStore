namespace Domain.Abstractions;

/// <summary>
/// Определяет интерфейс для всех событий домена.
/// События домена представляют важные изменения состояния в доменной модели.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Получает дату и время создания события.
    /// </summary>
    DateTime OccurredOn { get; }
}
