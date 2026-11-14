using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.UpdateCatalogAttribute;

public sealed class UpdateCatalogAttributeValidator : AbstractValidator<UpdateCatalogAttributeCommand>
{
    public UpdateCatalogAttributeValidator()
    {
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.NameUk).NotEmpty().MaximumLength(CatalogAttribute.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("nameUk");
        RuleFor(command => command.Request.NameRu).MaximumLength(CatalogAttribute.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("nameRu");
        RuleFor(command => command.Request.SortOrder).InclusiveBetween(0, 100_000)
            .When(command => command.Request is not null).OverridePropertyName("sortOrder");
    }
}
