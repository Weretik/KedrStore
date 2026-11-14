using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.CreateMediaFolder;

public sealed class CreateMediaFolderValidator : AbstractValidator<CreateMediaFolderCommand>
{
    public CreateMediaFolderValidator()
    {
        MediaFolderInputValidator.NameRules(RuleFor(x => x.Name));
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
    }
}
