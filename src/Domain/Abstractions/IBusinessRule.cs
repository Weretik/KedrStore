namespace Domain.Abstractions;

/// <summary>
/// Определяет интерфейс для бизнес-правил домена.
/// Бизнес-правила используются для валидации инвариантов домена.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Проверяет, соблюдается ли бизнес-правило.
    /// </summary>
    /// <returns>True, если правило соблюдается; в противном случае - false.</returns>
    bool IsBroken();

    /// <summary>
    /// Получает сообщение об ошибке, которое должно быть показано, когда правило нарушено.
    /// </summary>
    string Message { get; }
}
