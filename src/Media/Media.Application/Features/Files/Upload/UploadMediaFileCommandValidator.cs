using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Upload;

public sealed class UploadMediaFileCommandValidator : AbstractValidator<UploadMediaFileCommand>
{
    public UploadMediaFileCommandValidator()
    {
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        MediaUploadRules.FileRules(RuleFor(x => x.File)).OverridePropertyName("file");
    }
}
