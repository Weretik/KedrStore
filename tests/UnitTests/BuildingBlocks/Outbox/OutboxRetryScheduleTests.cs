using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Dispatch;

namespace UnitTests.BuildingBlocks.Outbox;

public sealed class OutboxRetryScheduleTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 5)]
    [InlineData(3, 15)]
    [InlineData(4, 30)]
    [InlineData(5, 60)]
    [InlineData(9, 60)]
    public void Delay_GrowsThenStaysHourly(int failedAttempts, int minutes)
        => Assert.Equal(TimeSpan.FromMinutes(minutes), OutboxRetrySchedule.DelayAfter(failedAttempts));

    [Fact]
    public void Outbox_GivesUp_AfterTenAttempts()
    {
        Assert.False(OutboxRetrySchedule.IsExhausted(9));
        Assert.True(OutboxRetrySchedule.IsExhausted(10));
    }

    [Fact]
    public void MessageTypes_RefuseTheSameNameTwice()
    {
        var first = new OutboxMessageType("sales.ping", typeof(PingMessage), (_, _, _, _) => Task.CompletedTask);
        var second = new OutboxMessageType("sales.ping", typeof(OtherMessage), (_, _, _, _) => Task.CompletedTask);

        Assert.Throws<InvalidOperationException>(() => new OutboxMessageTypes([first, second]));
    }

    [Fact]
    public void MessageTypes_RefuseAMessageWithoutHandler()
    {
        var types = new OutboxMessageTypes([]);

        Assert.Throws<InvalidOperationException>(() => types.Of(new PingMessage()));
    }

    private sealed record PingMessage : IOutboxMessage;

    private sealed record OtherMessage : IOutboxMessage;
}
