namespace Media.Application.Features.Files.Queries.GetMediaFiles;

public sealed class GetMediaFilesValidator : AbstractValidator<GetMediaFilesQuery>
{
    public const int MaxPageSize = 200;

    public GetMediaFilesValidator()
    {
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        RuleFor(x => x.InRoot).Equal(false).When(x => x.FolderId is not null)
            .WithMessage("Виберіть або папку, або корінь, не обидва одразу.");
        RuleFor(x => x.IncludeSubfolders).Equal(false).When(x => x.FolderId is null)
            .WithMessage("Щоб включити вкладені папки, виберіть папку.");
        RuleFor(x => x.Search).MaximumLength(MediaFile.FileNameMaxLength);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
