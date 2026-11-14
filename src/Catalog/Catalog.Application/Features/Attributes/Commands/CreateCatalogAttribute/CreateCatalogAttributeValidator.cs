using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;

public sealed class CreateCatalogAttributeValidator : AbstractValidator<CreateCatalogAttributeCommand>
{
    public CreateCatalogAttributeValidator()
    {
        RuleFor(command => command.Request).NotNull();
        When(command => command.Request is not null, () =>
        {
            RuleFor(command => command.Request.NameUk).NotEmpty().MaximumLength(CatalogAttribute.NameMaxLength)
                .OverridePropertyName("nameUk");
            RuleFor(command => command.Request.NameRu).MaximumLength(CatalogAttribute.NameMaxLength)
                .OverridePropertyName("nameRu");
            RuleFor(command => command.Request.Kind).Must(kind => CreateCatalogAttributeCommand.Kinds.Contains(kind))
                .WithMessage("Kind is enum, number, or yesno.").OverridePropertyName("kind");
            RuleFor(command => command.Request.Unit).MaximumLength(CatalogAttribute.UnitMaxLength)
                .OverridePropertyName("unit");
            RuleFor(command => command.Request.Code).MaximumLength(CatalogAttribute.CodeMaxLength)
                .Matches("^[a-z0-9_]+$").WithMessage("Code is lowercase Latin letters, digits, and _.")
                .When(command => !string.IsNullOrWhiteSpace(command.Request.Code)).OverridePropertyName("code");
            RuleFor(command => command.Request.Slug).MaximumLength(80).Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
                .When(command => !string.IsNullOrWhiteSpace(command.Request.Slug)).OverridePropertyName("slug");
            RuleFor(command => command.Request.SortOrder).InclusiveBetween(0, 100_000).OverridePropertyName("sortOrder");
        });
    }
}
