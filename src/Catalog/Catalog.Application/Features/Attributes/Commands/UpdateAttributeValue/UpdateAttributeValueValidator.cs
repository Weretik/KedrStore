using Catalog.Application.Features.Attributes.Shared;

namespace Catalog.Application.Features.Attributes.Commands.UpdateAttributeValue;

public sealed class UpdateAttributeValueValidator : AbstractValidator<UpdateAttributeValueCommand>
{
    public UpdateAttributeValueValidator()
    {
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.ValueId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        AttributeValueRules.NameAndSwatch(this, command => command.Request.NameUk, command => command.Request.NameRu,
            command => command.Request.SortOrder, command => command.Request.SwatchHex,
            command => command.Request.SwatchMediaFileId, command => command.Request is not null);
    }
}
