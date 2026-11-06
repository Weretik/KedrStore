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
        var task = WorkTask.Create(Owner, Owner, "  Тексти для ручок  ", "  ", null, null, Now);

        Assert.Equal(WorkTaskStatus.Planned, task.Status);
        Assert.Equal("Тексти для ручок", task.Title);
        Assert.Null(task.Details);
        Assert.True(task.IsCreatedByOwner);
    }

    [Fact]
    public void Create_Rejects_Empty_Title_And_Missing_Owner()
    {
        Assert.Throws<DomainException>(() => WorkTask.Create(Owner, Owner, "   ", null, null, null, Now));
        Assert.Throws<DomainException>(() => WorkTask.Create(Guid.Empty, Owner, "Задача", null, null, null, Now));
        Assert.Throws<DomainException>(() => WorkTask.Create(Owner, Owner, new string('a', WorkTask.TitleMaxLength + 1), null, null, null, Now));
    }

    [Fact]
    public void ChangeStatus_Sets_Start_Once_And_Keeps_Result_Only_While_Done()
    {
        var task = WorkTask.Create(Owner, Owner, "Задача", null, null, null, Now);

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
        var task = WorkTask.Create(Owner, Guid.NewGuid(), "Від адміністратора", null, null, null, Now);
        task.ChangeStatus(WorkTaskStatus.Done, "Готово", Now.AddHours(1));
        task.ChangeStatus(WorkTaskStatus.Done, "Уточнено", Now.AddHours(2));

        Assert.Equal(Now.AddHours(1), task.CompletedAt);
        Assert.Equal("Уточнено", task.Result);
        Assert.False(task.IsCreatedByOwner);
    }

    [Fact]
    public void Steps_AreReplacedOnEdit_AndTickedByPosition()
    {
        var task = WorkTask.Create(Owner, Owner, "Категорія «Петлі»", null, null,
            [new WorkTaskStepInput(" Title ", false), new WorkTaskStepInput("Description", false)], Now);
        Assert.Equal(["Title", "Description"], task.Steps.Select(step => step.Title));

        Assert.True(task.SetStepDone(1, true, Now.AddHours(1)));
        Assert.False(task.SetStepDone(2, true, Now.AddHours(1)));
        Assert.True(task.Steps[1].IsDone);
        Assert.Equal(Now.AddHours(1), task.UpdatedAt);

        task.Edit("Категорія «Петлі»", null, null, [new WorkTaskStepInput("FAQ", true)], Now.AddHours(2));
        Assert.Equal("FAQ", Assert.Single(task.Steps).Title);
        Assert.Throws<DomainException>(() => task.Edit("Задача", null, null, [new WorkTaskStepInput(" ", false)], Now));
        Assert.Throws<DomainException>(() => task.Edit("Задача", null, null,
            Enumerable.Range(0, WorkTask.StepsMaxCount + 1).Select(i => new WorkTaskStepInput($"Крок {i}", false)).ToList(),
            Now));
    }

    [Fact]
    public void IsOverdue_OnlyForAnOpenTaskPastItsDueDay()
    {
        var due = new DateOnly(2026, 11, 6);
        var task = WorkTask.Create(Owner, Owner, "Задача", null, due, null, Now);

        Assert.False(task.IsOverdue(due));
        Assert.True(task.IsOverdue(due.AddDays(1)));

        task.ChangeStatus(WorkTaskStatus.Done, "Зроблено", Now);
        Assert.False(task.IsOverdue(due.AddDays(1)));
        Assert.False(WorkTask.Create(Owner, Owner, "Без строку", null, null, null, Now).IsOverdue(due.AddDays(9)));
    }
}
