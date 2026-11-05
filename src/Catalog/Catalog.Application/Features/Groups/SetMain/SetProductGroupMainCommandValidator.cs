using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.SetMain;

public sealed class SetProductGroupMainCommandValidator : AbstractValidator<SetProductGroupMainCommand>
{
    public SetProductGroupMainCommandValidator()
    {
        RuleFor(command => command.GroupId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.ProductId).GreaterThan(0)
            .When(command => command.Request is not null).OverridePropertyName("productId");
    }
}
