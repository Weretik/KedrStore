using Catalog.Application.Groups;
using Catalog.Domain.Entities.Attributes;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace UnitTests.Catalog.Groups;

public sealed class ProductGroupTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void GroupKey_IgnoresSpacesAndCase()
    {
        Assert.Equal("завіса kedr flush", ProductGroupKey.From("  Завіса   KEDR\tFLUSH "));
        Assert.Equal("Завіса KEDR FLUSH", ProductGroupKey.DisplayName("  Завіса   KEDR\tFLUSH "));
    }

    [Fact]
    public void AutomaticMain_DropsThePersonsChoice()
    {
        var group = ProductGroup.CreateAutomatic(ProductCategoryId.From(10), "Ручка", "ручка", "ruchka", Now);
        group.SetMainManually(ProductId.From(2), Now);

        Assert.True(group.IsMainManual);
        Assert.True(group.SetMainAutomatically(ProductId.From(3), Now));
        Assert.False(group.IsMainManual);
        Assert.False(group.SetMainAutomatically(ProductId.From(3), Now));
    }

    [Fact]
    public void PinnedMember_IsNotMovedAutomatically()
    {
        var member = ProductGroupMember.CreatePinned(ProductId.From(1), null, Now);

        Assert.False(member.MoveAutomatically(ProductGroupId.Create(5), Now));
        member.Unpin(Now);
        Assert.True(member.MoveAutomatically(ProductGroupId.Create(5), Now));
        Assert.Equal(ProductGroupId.Create(5), member.GroupId);
    }

    [Fact]
    public void SameVariantsReason_SurvivesTheImport_AndClearsBackToNoProblem()
    {
        var review = ProductAttributeReview.Create(ProductId.From(1), "Ручка", [], Now);

        Assert.True(review.SetSameVariants("#2", Now));
        Assert.Equal(AttributeReviewState.NeedsReview, review.State);
        Assert.False(review.ApplyAutomatic("Ручка", [], Now));
        Assert.Single(review.Reasons, reason => reason.Code == AttributeReviewReason.SameVariants);

        Assert.True(review.SetSameVariants(null, Now));
        Assert.Empty(review.Reasons);
        Assert.Equal(AttributeReviewState.Clear, review.State);
    }
}
