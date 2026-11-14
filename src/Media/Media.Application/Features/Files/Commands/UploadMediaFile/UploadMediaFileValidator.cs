using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Commands.UploadMediaFile;

public sealed class UploadMediaFileValidator : AbstractValidator<UploadMediaFileCommand>
{
    public UploadMediaFileValidator()
    {
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        MediaUploadRules.FileRules(RuleFor(x => x.File)).OverridePropertyName("file");
    }
}
