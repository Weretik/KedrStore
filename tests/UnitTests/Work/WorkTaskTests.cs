using BuildingBlocks.Domain.Exceptions;
using Work.Domain.Tasks;

namespace UnitTests.Work;

public sealed class WorkTaskTests
{
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Starts_In_Planned_With_Trimmed_Text()
    {
        var task = WorkTask.Create(Owner, Owner, "  Тексти для ручок  ", "  ", Now);

        Assert.Equal(WorkTaskStatus.Planned, task.Status);
        Assert.Equal("Тексти для ручок", task.Title);
        Assert.Null(task.Details);
        Assert.True(task.IsCreatedByOwner);
    }

    [Fact]
    public void Create_Rejects_Empty_Title_And_Missing_Owner()
    {
        Assert.Throws<DomainException>(() => WorkTask.Create(Owner, Owner, "   ", null, Now));
        Assert.Throws<DomainException>(() => WorkTask.Create(Guid.Empty, Owner, "Задача", null, Now));
        Assert.Throws<DomainException>(() => WorkTask.Create(Owner, Owner, new string('a', WorkTask.TitleMaxLength + 1), null, Now));
    }

    [Fact]
    public void ChangeStatus_Sets_Start_Once_And_Keeps_Result_Only_While_Done()
    {
        var task = WorkTask.Create(Owner, Owner, "Задача", null, Now);

        task.ChangeStatus(WorkTaskStatus.InProgress, "ignored", Now.AddHours(1));
        Assert.Equal(Now.AddHours(1), task.StartedAt);
        Assert.Null(task.Result);

        task.ChangeStatus(WorkTaskStatus.Done, "  12 категорій  ", Now.AddHours(2));
        Assert.Equal(Now.AddHours(1), task.StartedAt);
        Assert.Equal(Now.AddHours(2), task.CompletedAt);
        Assert.Equal("12 категорій", task.Result);

        task.ChangeStatus(WorkTaskStatus.Planned, null, Now.AddHours(3));
        Assert.Null(task.CompletedAt);
        Assert.Null(task.Result);
        Assert.Equal(Now.AddHours(1), task.StartedAt);
    }

    [Fact]
    public void ChangeStatus_Done_To_Done_Keeps_Completion_Time()
    {
        var task = WorkTask.Create(Owner, Guid.NewGuid(), "Від адміністратора", null, Now);
        task.ChangeStatus(WorkTaskStatus.Done, "Готово", Now.AddHours(1));
        task.ChangeStatus(WorkTaskStatus.Done, "Уточнено", Now.AddHours(2));

        Assert.Equal(Now.AddHours(1), task.CompletedAt);
        Assert.Equal("Уточнено", task.Result);
        Assert.False(task.IsCreatedByOwner);
    }
}
