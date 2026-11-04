using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Update;

public sealed class UpdateMediaFolderCommandValidator : AbstractValidator<UpdateMediaFolderCommand>
{
    public UpdateMediaFolderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        MediaFolderInputValidator.NameRules(RuleFor(x => x.Name));
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
    }
}
