using Catalog.Application.Features.Attributes.Common;

namespace Catalog.Application.Features.Attributes.UpdateValue;

public sealed class UpdateAttributeValueCommandValidator : AbstractValidator<UpdateAttributeValueCommand>
{
    public UpdateAttributeValueCommandValidator()
    {
        RuleFor(command => command.AttributeId).GreaterThan(0);
        RuleFor(command => command.ValueId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        AttributeValueRules.NameAndSwatch(this, command => command.Request.NameUk, command => command.Request.NameRu,
            command => command.Request.SortOrder, command => command.Request.SwatchHex,
            command => command.Request.SwatchMediaFileId, command => command.Request is not null);
    }
}
