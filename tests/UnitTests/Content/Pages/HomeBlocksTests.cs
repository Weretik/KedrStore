using BuildingBlocks.Domain.Identifiers;
using UnitTests.BuildingBlocks.Results;
using Content.Domain.Home;
namespace UnitTests.Content.Pages;

public sealed class HomeBlocksTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FaqItem_TrimsTexts_AndRequiresAllFour()
    {
        var item = HomeFaqItem.Create(new HomeFaqTexts(" Питання ", "Вопрос", "Відповідь", "Ответ"), 2, Now).Value;

        Assert.Equal("Питання", item.QuestionUk);
        Assert.Equal(2, item.Position);
        DomainAssert.Fails(item.Update(new HomeFaqTexts("Питання", " ", "Відповідь", "Ответ"), Now));
        DomainAssert.Fails(item.Update(new HomeFaqTexts(new string('x', HomeFaqItem.QuestionMaxLength + 1), "Вопрос", "a", "b"), Now));
    }

    [Fact]
    public void Brand_NeedsANameAndALibraryLogo()
    {
        var brand = HomeBrand.Create(" Korfad ", MediaFileId.FromStorage(12), 0, Now).Value;

        Assert.Equal("Korfad", brand.Name);
        Assert.Equal(12, brand.LogoMediaFileId.Value);
        DomainAssert.Fails(brand.Update("Korfad", MediaFileId.FromStorage(0), Now));
        DomainAssert.Fails(brand.Update("", MediaFileId.FromStorage(12), Now));
        DomainAssert.Fails(brand.MoveTo(-1, Now));
    }
}
