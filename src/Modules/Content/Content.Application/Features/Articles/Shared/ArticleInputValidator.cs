namespace Content.Application.Features.Articles.Shared;

/// <summary>A command that carries article input from the admin.</summary>
public interface IArticleInputCommand
{
    ArticleInput Input { get; }
}

/// <summary>
/// Shape of the admin input for create and update; uniqueness of the slug is checked by the handler.
/// Abstract, so validator scanning registers only the concrete command validators.
/// </summary>
public abstract class ArticleInputValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : IArticleInputCommand
{
    protected ArticleInputValidator(bool slugRequired)
    {
        RuleFor(x => x.Input).NotNull().OverridePropertyName("body");
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Slug)
                .Must(slug => Article.IsValidSlug(slug?.Trim()))
                .When(x => slugRequired || !string.IsNullOrWhiteSpace(x.Input.Slug))
                .WithMessage($"Slug: латинські літери a-z, цифри та одинарні дефіси, від 1 до {Article.SlugMaxLength} символів.")
                .OverridePropertyName("slug");
            RuleFor(x => x.Input.PublishedOn).NotEmpty().OverridePropertyName("publishedOn");
            RuleFor(x => x.Input.CoverMediaFileId).GreaterThan(0).When(x => x.Input.CoverMediaFileId is not null)
                .OverridePropertyName("coverMediaFileId");
            RuleFor(x => x.Input.RemoveCover).Equal(false).When(x => x.Input.CoverMediaFileId is not null)
                .WithMessage("Або завантажте обкладинку, або приберіть її, не обидва одразу.").OverridePropertyName("removeCover");
            RuleFor(x => x.Input.Texts)
                .Must(texts => texts is not null && texts.Count == Article.Languages.Count &&
                               Article.Languages.All(language => texts.Count(t => t?.Language == language) == 1))
                .WithMessage("Потрібні тексти українською та російською, по одному.")
                .OverridePropertyName("texts");
            RuleForEach(x => x.Input.Texts).ChildRules(text =>
            {
                text.RuleFor(t => t.Title).NotEmpty().MaximumLength(ArticleTranslation.TitleMaxLength);
                text.RuleFor(t => t.Excerpt).NotEmpty().MaximumLength(ArticleTranslation.ExcerptMaxLength);
                text.RuleFor(t => t.BodyHtml).NotEmpty().MaximumLength(ArticleTranslation.BodyHtmlMaxLength);
            }).When(x => x.Input.Texts is not null).OverridePropertyName("texts");
        });
    }
}
