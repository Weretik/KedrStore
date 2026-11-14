namespace Catalog.Application.Features.Groups.Commands.SetProductGroup;

public sealed class SetProductGroupValidator : AbstractValidator<SetProductGroupCommand>
{
    public SetProductGroupValidator()
    {
        RuleFor(command => command.ProductId).GreaterThan(0);
        RuleFor(command => command.Request).NotNull();
        RuleFor(command => command.Request.GroupId).GreaterThan(0)
            .When(command => command.Request?.GroupId is not null).OverridePropertyName("groupId");
        RuleFor(command => command.Request.GroupId).Null()
            .WithMessage("Для автоматичного групування групу не вказують.")
            .When(command => command.Request is { Automatic: true }).OverridePropertyName("groupId");
    }
}
