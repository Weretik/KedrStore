namespace Content.Application.Features.Home.Shared;

public sealed class HomeFaqInputValidator : AbstractValidator<HomeFaqInput>
{
    public HomeFaqInputValidator()
    {
        RuleFor(x => x.QuestionUk).NotEmpty().MaximumLength(HomeFaqItem.QuestionMaxLength);
        RuleFor(x => x.QuestionRu).NotEmpty().MaximumLength(HomeFaqItem.QuestionMaxLength);
        RuleFor(x => x.AnswerUk).NotEmpty().MaximumLength(HomeFaqItem.AnswerMaxLength);
        RuleFor(x => x.AnswerRu).NotEmpty().MaximumLength(HomeFaqItem.AnswerMaxLength);
    }
}

public sealed class HomeBrandInputValidator : AbstractValidator<HomeBrandInput>
{
    public HomeBrandInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(HomeBrand.NameMaxLength);
        RuleFor(x => x.LogoMediaFileId).GreaterThan(0);
    }
}

public sealed class HomeOrderValidator : AbstractValidator<IReadOnlyList<long>>
{
    public HomeOrderValidator()
    {
        RuleFor(x => x).NotNull()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Кожен рядок має бути лише один раз.")
            .OverridePropertyName("ids");
        RuleForEach(x => x).GreaterThan(0).OverridePropertyName("ids");
    }
}
