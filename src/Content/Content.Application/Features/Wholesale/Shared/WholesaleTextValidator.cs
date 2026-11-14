namespace Content.Application.Features.Wholesale.Shared;

/// <summary>The domain limits, reported per field so the admin form can mark it.</summary>
internal sealed class WholesaleTextValidator : AbstractValidator<WholesaleTextDto>
{
    public WholesaleTextValidator()
    {
        Required(x => x.HeroTitle, WholesalePageText.TitleMaxLength);
        Required(x => x.HeroLead, WholesalePageText.TextMaxLength);
        Required(x => x.ContactCardTitle, WholesalePageText.ShortTitleMaxLength);
        Required(x => x.ContactCardNote, WholesalePageText.ShortTextMaxLength);
        Required(x => x.AssortmentTitle, WholesalePageText.TitleMaxLength);
        Required(x => x.AssortmentIntro, WholesalePageText.ShortTextMaxLength);
        Required(x => x.CompanyTitle, WholesalePageText.TitleMaxLength);
        Required(x => x.CompanyText, WholesalePageText.TextMaxLength);
        Required(x => x.SelectionTitle, WholesalePageText.ShortTitleMaxLength);
        Required(x => x.SelectionText, WholesalePageText.ShortTextMaxLength);
        Required(x => x.WarehouseTitle, WholesalePageText.ShortTitleMaxLength);
        Required(x => x.WarehouseText, WholesalePageText.ShortTextMaxLength);
        Required(x => x.StepsTitle, WholesalePageText.TitleMaxLength);
        Required(x => x.FaqTitle, WholesalePageText.TitleMaxLength);
        Required(x => x.ClosingTitle, WholesalePageText.TitleMaxLength);

        RuleFor(x => x.Steps).NotNull().Must(steps => steps.Count == WholesalePageText.StepCount)
            .WithMessage($"Exactly {WholesalePageText.StepCount} steps are required.");
        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(x => x.Title).Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Required.").MaximumLength(WholesalePageText.ShortTitleMaxLength);
            step.RuleFor(x => x.Text).Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Required.").MaximumLength(WholesalePageText.ShortTextMaxLength);
        }).When(x => x.Steps is not null);

        RuleFor(x => x.Faq).NotNull()
            .Must(faq => faq.Count is > 0 and <= WholesalePageText.MaxFaqItems)
            .WithMessage($"From 1 to {WholesalePageText.MaxFaqItems} questions are required.");
        RuleForEach(x => x.Faq).ChildRules(item =>
        {
            item.RuleFor(x => x.Question).Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Required.").MaximumLength(WholesalePageText.QuestionMaxLength);
            item.RuleFor(x => x.Answer).Must(value => !string.IsNullOrWhiteSpace(value))
                .WithMessage("Required.").MaximumLength(WholesalePageText.AnswerMaxLength);
        }).When(x => x.Faq is not null);
    }

    private void Required(System.Linq.Expressions.Expression<Func<WholesaleTextDto, string>> field, int maxLength)
        => RuleFor(field).Must(value => !string.IsNullOrWhiteSpace(value)).WithMessage("Required.")
            .MaximumLength(maxLength);
}
