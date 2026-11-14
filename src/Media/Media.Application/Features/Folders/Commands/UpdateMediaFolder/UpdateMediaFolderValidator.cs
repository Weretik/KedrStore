using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Folders.Commands.UpdateMediaFolder;

public sealed class UpdateMediaFolderValidator : AbstractValidator<UpdateMediaFolderCommand>
{
    public UpdateMediaFolderValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        MediaFolderInputValidator.NameRules(RuleFor(x => x.Name));
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
    }
}
