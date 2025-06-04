namespace Domain.Abstractions;

/// <summary>
/// Определяет базовый интерфейс для всех сущностей домена.
/// Все сущности должны иметь идентификатор.
/// </summary>
public interface IEntity<out TId>
{
    /// <summary>
    /// Получает уникальный идентификатор сущности.
    /// </summary>
    TId Id { get; }
}
