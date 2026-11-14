using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Commands.RenameProductGroup;

public sealed class RenameProductGroupValidator : AbstractValidator<RenameProductGroupCommand>
{
    public RenameProductGroupValidator()
    {
        RuleFor(command => command.GroupId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
    }
}
