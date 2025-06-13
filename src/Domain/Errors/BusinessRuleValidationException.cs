namespace Domain.Errors;

/// <summary>
/// Исключение, выбрасываемое при нарушении бизнес-правила домена.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    /// <summary>
    /// Получает нарушенное бизнес-правило.
    /// </summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BusinessRuleValidationException"/>.
    /// </summary>
    /// <param name="brokenRule">Нарушенное бизнес-правило.</param>
    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
    }

    /// <summary>
    /// Получает информацию о нарушенном правиле в виде строки.
    /// </summary>
    /// <returns>Сообщение о нарушенном правиле.</returns>
    public override string ToString()
    {
        return $"{BrokenRule.GetType().Name}: {BrokenRule.Message}";
    }
}
