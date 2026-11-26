namespace Catalog.Application.Features.Categories.Commands.SetCategorySearchSettings;

public sealed class SetCategorySearchSettingsValidator : AbstractValidator<SetCategorySearchSettingsCommand>
{
    public SetCategorySearchSettingsValidator()
    {
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.ShareImageMediaFileId).GreaterThan(0).When(command => command.ShareImageMediaFileId is not null)
            .WithMessage("Оберіть зображення з медіатеки.").OverridePropertyName("shareImageMediaFileId");
    }
}
