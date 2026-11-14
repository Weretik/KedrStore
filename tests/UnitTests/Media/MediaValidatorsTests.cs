using BuildingBlocks.Application.Validation;
using Media.Application.Abstractions.Images;
using Media.Application.Features.Files.Commands.MoveMediaFiles;
using Media.Application.Features.Files.Commands.ReplaceMediaFile;
using Media.Application.Features.Files.Commands.UpdateMediaFile;
using Media.Application.Features.Files.Commands.UploadMediaFile;
using Media.Application.Features.Files.Queries.GetMediaFiles;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Media.Domain.Folders;

namespace UnitTests.Media;

/// <summary>Picture library input: file size, names, folder ids and list filters.</summary>
public sealed class MediaValidatorsTests
{
    public MediaValidatorsTests() => UkrainianValidationMessages.Use();

    private static MediaUpload Upload(long length) => new("photo.jpg", "image/jpeg", length, () => Stream.Null);

    [Theory]
    [InlineData(0L, false)]
    [InlineData(1L, true)]
    [InlineData(MediaUploadRules.MaxUploadBytes, true)]
    [InlineData(MediaUploadRules.MaxUploadBytes + 1, false)]
    public void Upload_AcceptsNonEmptyFilesUpTo15Mb(long length, bool valid)
    {
        var result = new UploadMediaFileValidator().Validate(new UploadMediaFileCommand(null, Upload(length)));

        Assert.Equal(valid, result.IsValid);
        if (!valid)
        {
            var error = Assert.Single(result.Errors);
            Assert.Equal("file", error.PropertyName);
            Assert.Equal("Зображення має бути не більше 15 МБ.", error.ErrorMessage);
        }
    }

    [Fact]
    public void Upload_WithoutFile_AsksForOne()
        => Assert.Equal("Додайте файл.", Assert.Single(new ReplaceMediaFileValidator()
            .Validate(new ReplaceMediaFileCommand(1, null!)).Errors).ErrorMessage);

    [Fact]
    public void Upload_RefusesANonPositiveFolder()
        => Assert.False(new UploadMediaFileValidator().Validate(new UploadMediaFileCommand(0, Upload(10))).IsValid);

    [Theory]
    [InlineData("   ", "Вкажіть назву.")]
    [InlineData("", "Вкажіть назву.")]
    public void Folder_NameCannotBeBlank(string name, string message)
        => Assert.Equal(message, Assert.Single(new CreateMediaFolderValidator()
            .Validate(new CreateMediaFolderCommand(name, null)).Errors).ErrorMessage);

    [Fact]
    public void Folder_NameLimitCountsTheTrimmedName()
    {
        var longest = new string('п', MediaFolder.NameMaxLength);

        Assert.True(new UpdateMediaFolderValidator().Validate(new UpdateMediaFolderCommand(1, $" {longest} ", null)).IsValid);
        Assert.False(new UpdateMediaFolderValidator().Validate(new UpdateMediaFolderCommand(1, longest + "п", null)).IsValid);
    }

    [Fact]
    public void UpdateFile_NeedsANameAndValidIds()
    {
        var result = new UpdateMediaFileValidator().Validate(new UpdateMediaFileCommand(0, " ", -1));

        Assert.Equal(["Id", "FileName", "FolderId"], result.Errors.Select(e => e.PropertyName));
    }

    [Fact]
    public void Move_LimitsTheBatchAndChecksEachId()
    {
        var validator = new MoveMediaFilesValidator();
        var tooMany = Enumerable.Range(1, MoveMediaFilesValidator.MaxFiles + 1).Select(i => (long)i).ToList();

        Assert.False(validator.Validate(new MoveMediaFilesCommand([], null)).IsValid);
        Assert.False(validator.Validate(new MoveMediaFilesCommand([1, 0], null)).IsValid);
        Assert.Equal("За раз можна перемістити не більше 500 файлів.",
            Assert.Single(validator.Validate(new MoveMediaFilesCommand(tooMany, null)).Errors).ErrorMessage);
        Assert.True(validator.Validate(new MoveMediaFilesCommand([1, 2], 3)).IsValid);
    }

    [Fact]
    public void List_FolderAndRootAreExclusive()
        => Assert.Equal("Виберіть або папку, або корінь, не обидва одразу.", Assert.Single(new GetMediaFilesValidator()
            .Validate(new GetMediaFilesQuery(5, InRoot: true, Search: null)).Errors).ErrorMessage);

    [Fact]
    public void List_SubfoldersNeedAFolder()
        => Assert.Equal("Щоб включити вкладені папки, виберіть папку.", Assert.Single(new GetMediaFilesValidator()
            .Validate(new GetMediaFilesQuery(null, InRoot: false, Search: null, IncludeSubfolders: true)).Errors).ErrorMessage);

    [Theory]
    [InlineData(0, 60, false)]
    [InlineData(1, 0, false)]
    [InlineData(1, GetMediaFilesValidator.MaxPageSize, true)]
    [InlineData(1, GetMediaFilesValidator.MaxPageSize + 1, false)]
    public void List_ChecksPaging(int page, int pageSize, bool valid)
        => Assert.Equal(valid, new GetMediaFilesValidator()
            .Validate(new GetMediaFilesQuery(null, false, null, page, pageSize)).IsValid);
}
