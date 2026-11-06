namespace Catalog.Application.Features.Attributes.DecideCode;

public sealed class DecideAttributeCodeCommandValidator : AbstractValidator<DecideAttributeCodeCommand>
{
    public DecideAttributeCodeCommandValidator()
    {
        RuleFor(command => command.AliasId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ValueId).GreaterThan(0)
            .When(command => command.Request?.ValueId is not null).OverridePropertyName("valueId");
        RuleFor(command => command.Request.Categories)
            .Must(categories => categories!.Select(item => item.CategoryId).Distinct().Count() == categories!.Count)
            .WithMessage("A category is listed twice.")
            .When(command => command.Request?.Categories is not null).OverridePropertyName("categories");
        RuleForEach(command => command.Request.Categories).ChildRules(category =>
        {
            category.RuleFor(item => item.CategoryId).GreaterThan(0);
            category.RuleFor(item => item.ValueId).GreaterThan(0).When(item => item.ValueId is not null);
        }).When(command => command.Request?.Categories is not null).OverridePropertyName("categories");
    }
}
