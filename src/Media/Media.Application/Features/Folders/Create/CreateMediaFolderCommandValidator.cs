using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.Create;

public sealed class CreateMediaFolderCommandValidator : AbstractValidator<CreateMediaFolderCommand>
{
    public CreateMediaFolderCommandValidator()
    {
        MediaFolderInputValidator.NameRules(RuleFor(x => x.Name));
        RuleFor(x => x.ParentId).GreaterThan(0).When(x => x.ParentId is not null);
    }
}
