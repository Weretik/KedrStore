using System.Globalization;
using FluentValidation;
using FluentValidation.Resources;

namespace BuildingBlocks.Application.Validation;

/// <summary>
/// Validation messages for users, in Ukrainian. The API returns each message under its field, so a message does not
/// repeat the field name. A rule's own <c>WithMessage</c> text is also written in Ukrainian; FluentValidation's
/// built-in Ukrainian texts cover any validator not listed here.
/// </summary>
public sealed class UkrainianValidationMessages : LanguageManager
{
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk");

    private static readonly IReadOnlyDictionary<string, string> Messages = new Dictionary<string, string>
    {
        ["NotEmptyValidator"] = "Заповніть це поле.",
        ["NotNullValidator"] = "Заповніть це поле.",
        ["EmptyValidator"] = "Це поле має бути порожнім.",
        ["NullValidator"] = "Це поле має бути порожнім.",
        ["MaximumLengthValidator"] = "Не більше {MaxLength} символів (зараз {TotalLength}).",
        ["MinimumLengthValidator"] = "Не менше {MinLength} символів (зараз {TotalLength}).",
        ["LengthValidator"] = "Від {MinLength} до {MaxLength} символів (зараз {TotalLength}).",
        ["ExactLengthValidator"] = "Рівно {MaxLength} символів (зараз {TotalLength}).",
        ["EmailValidator"] = "Введіть коректну електронну пошту.",
        ["RegularExpressionValidator"] = "Значення має невірний формат.",
        ["InclusiveBetweenValidator"] = "Значення має бути від {From} до {To}.",
        ["ExclusiveBetweenValidator"] = "Значення має бути більше {From} і менше {To}.",
        ["GreaterThanValidator"] = "Значення має бути більше {ComparisonValue}.",
        ["GreaterThanOrEqualValidator"] = "Значення має бути не менше {ComparisonValue}.",
        ["LessThanValidator"] = "Значення має бути менше {ComparisonValue}.",
        ["LessThanOrEqualValidator"] = "Значення має бути не більше {ComparisonValue}.",
        ["EqualValidator"] = "Значення має дорівнювати {ComparisonValue}.",
        ["NotEqualValidator"] = "Значення не може дорівнювати {ComparisonValue}.",
        ["EnumValidator"] = "Виберіть одне з дозволених значень.",
        ["StringEnumValidator"] = "Виберіть одне з дозволених значень.",
        ["PredicateValidator"] = "Значення не підходить.",
        ["AsyncPredicateValidator"] = "Значення не підходить.",
        ["ScalePrecisionValidator"] = "Не більше {ExpectedPrecision} цифр, з них {ExpectedScale} після коми.",
        ["CreditCardValidator"] = "Введіть коректний номер картки.",
    };

    private static readonly object Gate = new();

    public UkrainianValidationMessages() => Culture = Ukrainian;

    /// <summary>Makes every validator answer in Ukrainian. Safe to call more than once.</summary>
    public static void Use()
    {
        lock (Gate)
        {
            if (ValidatorOptions.Global.LanguageManager is not UkrainianValidationMessages)
                ValidatorOptions.Global.LanguageManager = new UkrainianValidationMessages();
        }
    }

    public override string GetString(string key, CultureInfo? culture = null)
        => Messages.TryGetValue(key, out var message) ? message : base.GetString(key, Ukrainian);
}
