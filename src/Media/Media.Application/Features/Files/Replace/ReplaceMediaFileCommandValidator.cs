using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Replace;

public sealed class ReplaceMediaFileCommandValidator : AbstractValidator<ReplaceMediaFileCommand>
{
    public ReplaceMediaFileCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        MediaUploadRules.FileRules(RuleFor(x => x.File)).OverridePropertyName("file");
    }
}
