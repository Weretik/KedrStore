using Catalog.Domain.Entities.Groups;

namespace Catalog.Application.Features.Groups.Rename;

public sealed class RenameProductGroupCommandValidator : AbstractValidator<RenameProductGroupCommand>
{
    public RenameProductGroupCommandValidator()
    {
        RuleFor(command => command.GroupId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.Name).NotEmpty().MaximumLength(ProductGroup.NameMaxLength)
            .When(command => command.Request is not null).OverridePropertyName("name");
    }
}
