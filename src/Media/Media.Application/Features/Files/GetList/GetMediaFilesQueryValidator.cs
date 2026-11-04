namespace Media.Application.Features.Files.GetList;

public sealed class GetMediaFilesQueryValidator : AbstractValidator<GetMediaFilesQuery>
{
    public const int MaxPageSize = 200;

    public GetMediaFilesQueryValidator()
    {
        RuleFor(x => x.FolderId).GreaterThan(0).When(x => x.FolderId is not null);
        RuleFor(x => x.InRoot).Equal(false).When(x => x.FolderId is not null)
            .WithMessage("Use either folderId or inRoot, not both.");
        RuleFor(x => x.Search).MaximumLength(MediaFile.FileNameMaxLength);
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}
