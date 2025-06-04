using Domain.Abstractions;

namespace Domain.Common;

/// <summary>
/// Базовая реализация интерфейса IBusinessRule.
/// Предоставляет основу для создания бизнес-правил домена.
/// </summary>
public abstract class BusinessRule : IBusinessRule
{
    /// <summary>
    /// Получает сообщение об ошибке, которое должно быть показано, когда правило нарушено.
    /// </summary>
    public abstract string Message { get; }

    /// <summary>
    /// Проверяет, соблюдается ли бизнес-правило.
    /// </summary>
    /// <returns>True, если правило соблюдается; в противном случае - false.</returns>
    public abstract bool IsBroken();

    /// <summary>
    /// Проверяет, соблюдается ли бизнес-правило.
    /// </summary>
    /// <returns>True, если правило соблюдается; в противном случае - false.</returns>
    public bool IsValid() => !IsBroken();
}
