using Catalog.Application.Features.Attributes.Shared;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Commands.CreateAttributeValue;

public sealed class CreateAttributeValueValidator : AbstractValidator<CreateAttributeValueCommand>
{
    public CreateAttributeValueValidator()
    {
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        AttributeValueRules.NameAndSwatch(this, command => command.Request.NameUk, command => command.Request.NameRu,
            command => command.Request.SortOrder, command => command.Request.SwatchHex,
            command => command.Request.SwatchMediaFileId, command => command.Request is not null);
        RuleFor(command => command.Request.Code).MaximumLength(AttributeValue.CodeMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("code");
        RuleFor(command => command.Request.Slug).MaximumLength(80).Matches("^[a-z0-9]+(-[a-z0-9]+)*$")
            .When(command => !string.IsNullOrWhiteSpace(command.Request?.Slug)).OverridePropertyName("slug");
    }
}
