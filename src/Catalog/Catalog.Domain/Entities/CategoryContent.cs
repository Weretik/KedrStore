using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

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
        if (categoryId.Value <= 0)
            throw new DomainException(CategoryContentErrors.CategoryIdRequired());

        var normalized = language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!Languages.Contains(normalized))
            throw new DomainException(CategoryContentErrors.LanguageInvalid(language));

        return new CategoryContent { CategoryId = categoryId, Language = normalized };
    }

    /// <summary>Replaces every field and the FAQ. <paramref name="bodyHtml"/> must already be sanitized.</summary>
    public void Replace(string? seoTitle, string? metaDescription, string? h1, string? bodyHtml,
        IReadOnlyList<(string Question, string Answer)> faq, string updatedBy, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(faq);
        ArgumentException.ThrowIfNullOrWhiteSpace(updatedBy);

        SeoTitle = Optional(seoTitle, SeoTitleMaxLength, nameof(SeoTitle));
        MetaDescription = Optional(metaDescription, MetaDescriptionMaxLength, nameof(MetaDescription));
        H1 = Optional(h1, H1MaxLength, nameof(H1));
        BodyHtml = Optional(bodyHtml, BodyHtmlMaxLength, nameof(BodyHtml));

        if (faq.Count > FaqMaxItems)
            throw new DomainException(CategoryContentErrors.TooManyFaqItems(faq.Count));

        _faq.Clear();
        for (var index = 0; index < faq.Count; index++)
        {
            var question = Required(faq[index].Question, CategoryFaqItem.QuestionMaxLength, "Faq.Question");
            var answer = Required(faq[index].Answer, CategoryFaqItem.AnswerMaxLength, "Faq.Answer");
            _faq.Add(CategoryFaqItem.Create(index, question, answer));
        }

        UpdatedBy = updatedBy.Trim();
        UpdatedAt = now;
    }

    public bool IsEmpty => SeoTitle is null && MetaDescription is null && H1 is null && BodyHtml is null && _faq.Count == 0;

    private static string? Optional(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(CategoryContentErrors.TooLong(field, maxLength, trimmed.Length));

        return trimmed;
    }

    private static string Required(string? value, int maxLength, string field)
        => Optional(value, maxLength, field) ?? throw new DomainException(CategoryContentErrors.Required(field));
}
