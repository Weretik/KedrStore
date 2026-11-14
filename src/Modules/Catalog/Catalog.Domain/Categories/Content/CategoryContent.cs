namespace Catalog.Domain.Categories.Content;

/// <summary>Admin-written SEO text and FAQ of one category in one language. The 1C import never touches it.</summary>
public class CategoryContent : IAggregateRoot
{
    public const int SeoTitleMaxLength = 70;
    public const int MetaDescriptionMaxLength = 160;
    public const int H1MaxLength = 120;
    public const int BodyHtmlMaxLength = 20_000;
    public const int FaqMaxItems = 20;
    public const int UpdatedByMaxLength = 64;
    public static readonly IReadOnlyList<string> Languages = ["uk", "ru"];

    private readonly List<CategoryFaqItem> _faq = [];

    public ProductCategoryId CategoryId { get; private set; }
    public string Language { get; private set; } = null!;
    public string? SeoTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? H1 { get; private set; }
    public string? BodyHtml { get; private set; }
    public IReadOnlyList<CategoryFaqItem> Faq => _faq;
    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; } = null!;

    private CategoryContent() { }

    public static CategoryContent Create(ProductCategoryId categoryId, string language)
    {
        // The category id and the language come from the validated route.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(categoryId.Value, nameof(categoryId));
        var normalized = language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!Languages.Contains(normalized))
            throw new ArgumentOutOfRangeException(nameof(language), language, "Expected uk or ru.");

        return new CategoryContent { CategoryId = categoryId, Language = normalized };
    }

    /// <summary>Replaces every field and the FAQ. <paramref name="bodyHtml"/> must already be sanitized.</summary>
    public DomainResult Replace(string? seoTitle, string? metaDescription, string? h1, string? bodyHtml,
        IReadOnlyList<(string Question, string Answer)> faq, string updatedBy, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(faq);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);

        var checks = new DomainChecks();
        var checkedSeoTitle = checks.Take(Optional(seoTitle, SeoTitleMaxLength, nameof(SeoTitle)));
        var checkedMeta = checks.Take(Optional(metaDescription, MetaDescriptionMaxLength, nameof(MetaDescription)));
        var checkedH1 = checks.Take(Optional(h1, H1MaxLength, nameof(H1)));
        var checkedBody = checks.Take(Optional(bodyHtml, BodyHtmlMaxLength, nameof(BodyHtml)));
        if (faq.Count > FaqMaxItems)
            checks.Check(CategoryContentErrors.TooManyFaqItems(faq.Count));

        var items = new List<CategoryFaqItem>();
        for (var index = 0; index < faq.Count && !checks.Failed; index++)
        {
            var question = checks.Take(Required(faq[index].Question, CategoryFaqItem.QuestionMaxLength, "Faq.Question"));
            var answer = checks.Take(Required(faq[index].Answer, CategoryFaqItem.AnswerMaxLength, "Faq.Answer"));
            if (!checks.Failed)
                items.Add(CategoryFaqItem.Create(index, question, answer));
        }

        if (checks.Error is { } error)
            return error;

        SeoTitle = checkedSeoTitle;
        MetaDescription = checkedMeta;
        H1 = checkedH1;
        BodyHtml = checkedBody;
        _faq.Clear();
        _faq.AddRange(items);
        UpdatedBy = updatedBy.Trim();
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public bool IsEmpty => SeoTitle is null && MetaDescription is null && H1 is null && BodyHtml is null && _faq.Count == 0;

    private static DomainResult<string?> Optional(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (string?)null;

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? CategoryContentErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }

    private static DomainResult<string> Required(string? value, int maxLength, string field)
    {
        var optional = Optional(value, maxLength, field);
        if (optional.IsFailure)
            return optional.Error!;
        return optional.Value is { } text ? text : CategoryContentErrors.Required(field);
    }
}
