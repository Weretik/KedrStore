using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Commands.SaveCategoryContent;

public sealed class SaveCategoryContentValidator : AbstractValidator<SaveCategoryContentCommand>
{
    public SaveCategoryContentValidator()
    {
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Language)
            .Must(language => CategoryContent.Languages.Contains(language))
            .WithMessage("Language must be 'uk' or 'ru'.");
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.SeoTitle).MaximumLength(CategoryContent.SeoTitleMaxLength).OverridePropertyName("seoTitle");
        RuleFor(x => x.Request.MetaDescription).MaximumLength(CategoryContent.MetaDescriptionMaxLength)
            .OverridePropertyName("metaDescription");
        RuleFor(x => x.Request.H1).MaximumLength(CategoryContent.H1MaxLength).OverridePropertyName("h1");
        RuleFor(x => x.Request.BodyHtml).MaximumLength(CategoryContent.BodyHtmlMaxLength).OverridePropertyName("bodyHtml");

        RuleFor(x => x.Request.Faq!.Count)
            .LessThanOrEqualTo(CategoryContent.FaqMaxItems)
            .When(x => x.Request?.Faq is not null)
            .OverridePropertyName("faq");

        RuleForEach(x => x.Request.Faq).ChildRules(item =>
        {
            item.RuleFor(x => x.Question).NotEmpty().MaximumLength(CategoryFaqItem.QuestionMaxLength);
            item.RuleFor(x => x.Answer).NotEmpty().MaximumLength(CategoryFaqItem.AnswerMaxLength);
        }).OverridePropertyName("faq");
    }
}
