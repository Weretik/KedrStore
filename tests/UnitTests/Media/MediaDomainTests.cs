using UnitTests.BuildingBlocks.Results;
using Media.Application.Features.Files.Queries.GetMediaFiles;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Domain.Files;
using Media.Domain.Folders;

namespace UnitTests.Media;

public sealed class MediaDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Folder_NameIsTrimmed_AndMustNotBeEmptyOrTooLong()
    {
        Assert.Equal("Акції", MediaFolder.Create("  Акції  ", null, Now).Value.Name);
        DomainAssert.Fails(MediaFolder.Create("   ", null, Now));
        DomainAssert.Fails(MediaFolder.Create(new string('a', MediaFolder.NameMaxLength + 1), null, Now));
    }

    [Fact]
    public void File_Replace_KeepsNameAndReturnsOldKey()
    {
        var file = MediaFile.Create(null, "731.jpg", new MediaFileContent("/products/731.jpg", "image/jpeg", 10, null, null), Now).Value;
        Assert.Equal("products/731.jpg", file.StorageKey);

        var later = Now.AddDays(1);
        var old = file.Replace(new MediaFileContent("library/2026/11/a.webp", "image/webp", 20, 800, 600), later).Value;

        Assert.Equal("products/731.jpg", old);
        Assert.Equal("library/2026/11/a.webp", file.StorageKey);
        Assert.Equal("731.jpg", file.FileName);
        Assert.Equal((800, 600), (file.Width!.Value, file.Height!.Value));
        Assert.Equal(later, file.UpdatedAt);
    }

    [Fact]
    public void File_RejectsEmptyKeyAndBadDimensions()
    {
        DomainAssert.Fails(MediaFile.Create(null, "a.jpg", new MediaFileContent(" ", "image/jpeg", 1, null, null), Now));
        DomainAssert.Fails(MediaFile.Create(null, "a.jpg", new MediaFileContent("a.jpg", "image/jpeg", 1, 0, 10), Now));
        DomainAssert.Fails(MediaFile.Create(null, " ", new MediaFileContent("a.jpg", "image/jpeg", 1, null, null), Now));
    }

    [Fact]
    public void Validators_RejectBadInput()
    {
        Assert.False(new CreateMediaFolderValidator().Validate(new CreateMediaFolderCommand("", 0)).IsValid);
        Assert.True(new CreateMediaFolderValidator().Validate(new CreateMediaFolderCommand("Схеми", null)).IsValid);

        var list = new GetMediaFilesValidator();
        Assert.False(list.Validate(new GetMediaFilesQuery(5, true, null)).IsValid);
        Assert.False(list.Validate(new GetMediaFilesQuery(null, false, null, PageSize: 500)).IsValid);
        Assert.True(list.Validate(new GetMediaFilesQuery(null, true, "731")).IsValid);
        Assert.False(list.Validate(new GetMediaFilesQuery(null, false, null, IncludeSubfolders: true)).IsValid);
        Assert.True(list.Validate(new GetMediaFilesQuery(5, false, null, IncludeSubfolders: true)).IsValid);
    }
}
