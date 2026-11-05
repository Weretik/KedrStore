namespace Content.Application.Features.SiteSeo.Admin.Update;

public sealed class UpdatePageSeoCommandValidator : AbstractValidator<UpdatePageSeoCommand>
{
    public UpdatePageSeoCommandValidator()
    {
        RuleFor(x => x.Page).Must(SitePages.IsKnown).WithMessage("Unknown site page.").OverridePropertyName("page");
        Text(x => x.TitleUk, "titleUk", PageSeo.TitleMaxLength);
        Text(x => x.TitleRu, "titleRu", PageSeo.TitleMaxLength);
        Text(x => x.DescriptionUk, "descriptionUk", PageSeo.DescriptionMaxLength);
        Text(x => x.DescriptionRu, "descriptionRu", PageSeo.DescriptionMaxLength);
    }

    private void Text(System.Linq.Expressions.Expression<Func<UpdatePageSeoCommand, string?>> field, string name,
        int maxLength) =>
        RuleFor(field).Must(value => (value?.Trim().Length ?? 0) <= maxLength)
            .WithMessage($"At most {maxLength} characters.").OverridePropertyName(name);
}
