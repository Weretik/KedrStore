using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.SetForProduct;

public sealed class SetProductGroupCommandValidator : AbstractValidator<SetProductGroupCommand>
{
    public SetProductGroupCommandValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.GroupId).GreaterThan(0)
            .When(command => command.Request?.GroupId is not null).OverridePropertyName("groupId");
        RuleFor(command => command.Request.GroupId).Null()
            .WithMessage("Automatic grouping does not take a group.")
            .When(command => command.Request is { Automatic: true }).OverridePropertyName("groupId");
    }
}
