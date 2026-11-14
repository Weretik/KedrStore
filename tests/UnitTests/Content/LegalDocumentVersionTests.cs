using BuildingBlocks.Domain.Exceptions;
using Content.Domain.Documents;

namespace UnitTests.Content;

public sealed class LegalDocumentVersionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 0, 0, 0, TimeSpan.Zero);

    private static LegalDocumentTextInput[] Texts(string title = " Публічна оферта ") =>
    [
        new("ru", "Публичная оферта", "<p>Текст</p>"),
        new("uk", title, "<p>Текст</p>"),
    ];

    [Fact]
    public void CreateDraft_KeepsBothLanguages_AndHasNoNumberOrDate()
    {
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, " admin@kedr ");

        Assert.Equal(LegalDocumentStatus.Draft, draft.Status);
        Assert.Null(draft.Number);
        Assert.Null(draft.EffectiveFrom);
        Assert.Equal("Публічна оферта", draft.Text("uk").Title);
        Assert.Equal("admin@kedr", draft.UpdatedBy);
    }

    [Fact]
    public void CreateDraft_WithoutRussian_Throws()
    {
        Assert.Throws<DomainException>(() => LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer,
            [new LegalDocumentTextInput("uk", "Оферта", "<p>x</p>")], Now, "admin"));
    }

    [Fact]
    public void CreateDraft_WithEmptyBody_Throws()
    {
        Assert.Throws<DomainException>(() => LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer,
            [new("uk", "Оферта", " "), new("ru", "Оферта", "<p>x</p>")], Now, "admin"));
    }

    [Fact]
    public void Publish_SetsNumberAndDate_AndThenTheTextIsReadOnly()
    {
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.DeliveryPayment, Texts(), Now, "admin");

        draft.Publish(2, new DateOnly(2026, 11, 10), Now.AddHours(1), "director");

        Assert.Equal(LegalDocumentStatus.Published, draft.Status);
        Assert.Equal(2, draft.Number);
        Assert.Equal(new DateOnly(2026, 11, 10), draft.EffectiveFrom);
        Assert.Equal("director", draft.PublishedBy);
        Assert.Throws<DomainException>(() => draft.UpdateDraft(Texts("Нова"), Now, "admin"));
        Assert.Throws<DomainException>(() => draft.Publish(3, new DateOnly(2026, 11, 11), Now, "admin"));
    }

    [Fact]
    public void Archive_OnlyFromPublished()
    {
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PrivacyPolicy, Texts(), Now, "admin");
        Assert.Throws<DomainException>(draft.Archive);

        draft.Publish(1, new DateOnly(2026, 11, 3), Now, "admin");
        draft.Archive();

        Assert.Equal(LegalDocumentStatus.Archived, draft.Status);
        Assert.Equal(1, draft.Number);
    }

    [Fact]
    public void IsDraft_IsTrueOnlyBeforePublishing()
    {
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");
        Assert.True(draft.IsDraft);

        draft.Publish(1, new DateOnly(2026, 11, 3), Now, "admin");

        Assert.False(draft.IsDraft);
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    public void AllowsEffectiveFrom_WithoutAnEditionOnTheSite_AcceptsAnyDate(int day, bool expected)
        => Assert.Equal(expected, LegalDocumentVersion.AllowsEffectiveFrom(new DateOnly(2026, 11, day), current: null));

    [Theory]
    [InlineData(2, false)]
    [InlineData(3, true)]
    [InlineData(4, true)]
    public void AllowsEffectiveFrom_RejectsADateBeforeTheEditionOnTheSite(int day, bool expected)
    {
        var current = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");
        current.Publish(1, new DateOnly(2026, 11, 3), Now, "admin");

        Assert.Equal(expected, LegalDocumentVersion.AllowsEffectiveFrom(new DateOnly(2026, 11, day), current));
    }

    [Fact]
    public void PublishReplacing_ArchivesTheCurrentEdition_AndTakesTheNextNumber()
    {
        var current = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");
        current.Publish(2, new DateOnly(2026, 11, 3), Now, "admin");
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts("Нова"), Now, "admin");

        draft.PublishReplacing(current, lastNumber: 2, new DateOnly(2026, 11, 10), Now, "director");

        Assert.Equal(LegalDocumentStatus.Archived, current.Status);
        Assert.Equal(LegalDocumentStatus.Published, draft.Status);
        Assert.Equal(3, draft.Number);
    }

    [Fact]
    public void PublishReplacing_FirstEdition_GetsNumberOne()
    {
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.ReturnsPolicy, Texts(), Now, "admin");

        draft.PublishReplacing(current: null, lastNumber: 0, new DateOnly(2026, 11, 10), Now, "admin");

        Assert.Equal(1, draft.Number);
    }

    [Fact]
    public void PublishReplacing_RejectsADateBeforeTheCurrentEdition()
    {
        var current = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");
        current.Publish(1, new DateOnly(2026, 11, 3), Now, "admin");
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");

        var error = Assert.Throws<DomainException>(() =>
            draft.PublishReplacing(current, 1, new DateOnly(2026, 11, 2), Now, "admin"));

        Assert.Equal("Content.Document.EffectiveFrom.TooEarly", error.Error.Code);
        Assert.Equal(LegalDocumentStatus.Published, current.Status);
    }

    [Fact]
    public void PublishReplacing_RejectsAnotherKindAsTheCurrentEdition()
    {
        var current = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PrivacyPolicy, Texts(), Now, "admin");
        current.Publish(1, new DateOnly(2026, 11, 3), Now, "admin");
        var draft = LegalDocumentVersion.CreateDraft(LegalDocumentKind.PublicOffer, Texts(), Now, "admin");

        Assert.Throws<ArgumentException>(() =>
            draft.PublishReplacing(current, 1, new DateOnly(2026, 11, 10), Now, "admin"));
    }
}
