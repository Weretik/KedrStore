using Media.Application.Features.Files.Shared;

namespace Media.Application.Features.Files.Commands.ReplaceMediaFile;

public sealed class ReplaceMediaFileValidator : AbstractValidator<ReplaceMediaFileCommand>
{
    public ReplaceMediaFileValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        MediaUploadRules.FileRules(RuleFor(x => x.File)).OverridePropertyName("file");
    }
}
