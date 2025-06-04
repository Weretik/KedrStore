using Domain.Abstractions;

namespace Domain.Common;

/// <summary>
/// Утилитарный класс для проверки бизнес-правил и генерации исключений при их нарушении.
/// </summary>
public static class RuleChecker
{
    /// <summary>
    /// Проверяет бизнес-правило и выбрасывает исключение, если правило нарушено.
    /// </summary>
    /// <param name="rule">Бизнес-правило для проверки.</param>
    /// <exception cref="BusinessRuleValidationException">Выбрасывается, если правило нарушено.</exception>
    public static void CheckRule(IBusinessRule rule)
    {
        if (rule.IsBroken())
        {
            throw new BusinessRuleValidationException(rule);
        }
    }
}
