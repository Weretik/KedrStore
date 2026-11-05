namespace Content.Domain.Wholesale;

/// <summary>What one language of the page «Співпраця» says, block by block, from the top.</summary>
public sealed record WholesalePageTextValues(
    string HeroTitle,
    string HeroLead,
    string ContactCardTitle,
    string ContactCardNote,
    string AssortmentTitle,
    string AssortmentIntro,
    string CompanyTitle,
    string CompanyText,
    string SelectionTitle,
    string SelectionText,
    string WarehouseTitle,
    string WarehouseText,
    string StepsTitle,
    IReadOnlyList<(string Title, string Text)> Steps,
    string FaqTitle,
    IReadOnlyList<(string Question, string Answer)> Faq,
    string ClosingTitle);

/// <summary>
/// One language of the page, stored as a JSON document. The design has exactly <see cref="StepCount"/> order steps
/// and 1 to <see cref="MaxFaqItems"/> questions; every text is plain (no HTML) and required.
/// </summary>
public sealed class WholesalePageText
{
    public const int StepCount = 3;
    public const int MaxFaqItems = 12;
    public const int TitleMaxLength = 120;
    public const int ShortTitleMaxLength = 80;
    public const int ShortTextMaxLength = 200;
    public const int TextMaxLength = 600;
    public const int QuestionMaxLength = 200;
    public const int AnswerMaxLength = 1000;

    private List<WholesaleStep> _steps = [];
    private List<WholesaleFaqItem> _faq = [];

    public string HeroTitle { get; private set; } = null!;
    public string HeroLead { get; private set; } = null!;
    public string ContactCardTitle { get; private set; } = null!;
    public string ContactCardNote { get; private set; } = null!;
    public string AssortmentTitle { get; private set; } = null!;
    public string AssortmentIntro { get; private set; } = null!;

    /// <summary>Follows the company name on the site («Class Kedr — …»), so it may start in lower case.</summary>
    public string CompanyTitle { get; private set; } = null!;
    public string CompanyText { get; private set; } = null!;
    public string SelectionTitle { get; private set; } = null!;
    public string SelectionText { get; private set; } = null!;
    public string WarehouseTitle { get; private set; } = null!;
    public string WarehouseText { get; private set; } = null!;
    public string StepsTitle { get; private set; } = null!;
    public IReadOnlyList<WholesaleStep> Steps => _steps;
    public string FaqTitle { get; private set; } = null!;
    public IReadOnlyList<WholesaleFaqItem> Faq => _faq;
    public string ClosingTitle { get; private set; } = null!;

    private WholesalePageText() { }

    public static WholesalePageText Create(WholesalePageTextValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Steps is null || values.Steps.Count != StepCount)
            throw new DomainException(WholesaleErrors.StepCount(StepCount, values.Steps?.Count ?? 0));
        if (values.Faq is null || values.Faq.Count is 0 or > MaxFaqItems)
            throw new DomainException(WholesaleErrors.FaqCount(MaxFaqItems, values.Faq?.Count ?? 0));

        return new WholesalePageText
        {
            HeroTitle = Text(values.HeroTitle, nameof(HeroTitle), TitleMaxLength),
            HeroLead = Text(values.HeroLead, nameof(HeroLead), TextMaxLength),
            ContactCardTitle = Text(values.ContactCardTitle, nameof(ContactCardTitle), ShortTitleMaxLength),
            ContactCardNote = Text(values.ContactCardNote, nameof(ContactCardNote), ShortTextMaxLength),
            AssortmentTitle = Text(values.AssortmentTitle, nameof(AssortmentTitle), TitleMaxLength),
            AssortmentIntro = Text(values.AssortmentIntro, nameof(AssortmentIntro), ShortTextMaxLength),
            CompanyTitle = Text(values.CompanyTitle, nameof(CompanyTitle), TitleMaxLength),
            CompanyText = Text(values.CompanyText, nameof(CompanyText), TextMaxLength),
            SelectionTitle = Text(values.SelectionTitle, nameof(SelectionTitle), ShortTitleMaxLength),
            SelectionText = Text(values.SelectionText, nameof(SelectionText), ShortTextMaxLength),
            WarehouseTitle = Text(values.WarehouseTitle, nameof(WarehouseTitle), ShortTitleMaxLength),
            WarehouseText = Text(values.WarehouseText, nameof(WarehouseText), ShortTextMaxLength),
            StepsTitle = Text(values.StepsTitle, nameof(StepsTitle), TitleMaxLength),
            _steps = values.Steps.Select(step => new WholesaleStep(
                Text(step.Title, "StepTitle", ShortTitleMaxLength),
                Text(step.Text, "StepText", ShortTextMaxLength))).ToList(),
            FaqTitle = Text(values.FaqTitle, nameof(FaqTitle), TitleMaxLength),
            _faq = values.Faq.Select(item => new WholesaleFaqItem(
                Text(item.Question, "Question", QuestionMaxLength),
                Text(item.Answer, "Answer", AnswerMaxLength))).ToList(),
            ClosingTitle = Text(values.ClosingTitle, nameof(ClosingTitle), TitleMaxLength),
        };
    }

    private static string Text(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
            throw new DomainException(WholesaleErrors.Required(field));
        return trimmed.Length <= maxLength
            ? trimmed
            : throw new DomainException(WholesaleErrors.TooLong(field, maxLength, trimmed.Length));
    }
}
