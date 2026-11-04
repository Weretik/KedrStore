using BuildingBlocks.Domain.Exceptions;
using Content.Domain.Home;

namespace UnitTests.Content;

public sealed class HomeBlocksTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void FaqItem_TrimsTexts_AndRequiresAllFour()
    {
        var item = HomeFaqItem.Create(new HomeFaqTexts(" Питання ", "Вопрос", "Відповідь", "Ответ"), 2, Now);

        Assert.Equal("Питання", item.QuestionUk);
        Assert.Equal(2, item.Position);
        Assert.Throws<DomainException>(() => item.Update(new HomeFaqTexts("Питання", " ", "Відповідь", "Ответ"), Now));
        Assert.Throws<DomainException>(() =>
            item.Update(new HomeFaqTexts(new string('x', HomeFaqItem.QuestionMaxLength + 1), "Вопрос", "a", "b"), Now));
    }

    [Fact]
    public void Brand_NeedsANameAndALibraryLogo()
    {
        var brand = HomeBrand.Create(" Korfad ", 12, 0, Now);

        Assert.Equal("Korfad", brand.Name);
        Assert.Equal(12, brand.LogoMediaFileId);
        Assert.Throws<DomainException>(() => brand.Update("Korfad", 0, Now));
        Assert.Throws<DomainException>(() => brand.Update("", 12, Now));
        Assert.Throws<DomainException>(() => brand.MoveTo(-1, Now));
    }
}
