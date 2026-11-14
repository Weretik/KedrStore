using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.DecideAttributeCode;

public sealed class DecideAttributeCodeValidator : AbstractValidator<DecideAttributeCodeCommand>
{
    private const string TwiceMessage = "Характеристика вказана двічі.";

    public DecideAttributeCodeValidator()
    {
        RuleFor(command => command.Request.Meanings).Must(OnePerAttribute!).WithMessage(TwiceMessage)
            .When(command => command.Request?.Meanings is not null).OverridePropertyName("meanings");
        RuleForEach(command => command.Request.Meanings).SetValidator(new CodeMeaningValidator())
            .When(command => command.Request?.Meanings is not null).OverridePropertyName("meanings");
        RuleFor(command => command.AliasId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ValueId).GreaterThan(0)
            .When(command => command.Request?.ValueId is not null).OverridePropertyName("valueId");
        RuleFor(command => command.Request.Categories)
            .Must(categories => categories!.Select(item => item.CategoryId).Distinct().Count() == categories!.Count)
            .WithMessage("Категорія вказана двічі.")
            .When(command => command.Request?.Categories is not null).OverridePropertyName("categories");
        RuleForEach(command => command.Request.Categories).ChildRules(category =>
        {
            category.RuleFor(item => item.CategoryId).GreaterThan(0);
            category.RuleFor(item => item.ValueId).GreaterThan(0).When(item => item.ValueId is not null);
            category.RuleFor(item => item.Meanings).Must(OnePerAttribute!).WithMessage(TwiceMessage)
                .When(item => item.Meanings is not null);
            category.RuleForEach(item => item.Meanings).SetValidator(new CodeMeaningValidator());
        }).When(command => command.Request?.Categories is not null).OverridePropertyName("categories");
    }

    private static bool OnePerAttribute(IReadOnlyList<CodeMeaningRequest> meanings)
        => meanings.Select(item => item.AttributeId).Distinct().Count() == meanings.Count;

    private sealed class CodeMeaningValidator : AbstractValidator<CodeMeaningRequest>
    {
        public CodeMeaningValidator()
        {
            RuleFor(item => item.AttributeId).GreaterThan(0);
            RuleFor(item => item.ValueId).GreaterThan(0);
        }
    }
}
