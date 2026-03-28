using FluentAssertions;
using Moq;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Services;
using TaskBoard.Domain;

namespace TaskBoard.Tests.Application.Services;

public class ListTaskTests
{


    private const string OwnerId = "owner-1";

    private readonly Mock<ITaskRepository> _repoMock = new();

    private ListTask CreateSut() => new(_repoMock.Object);

    private static TaskItem MakeTask(
        string title = "Test task",
        Priority priority = Priority.Low,
        Status status = Status.Todo,
        IEnumerable<Tag>? tags = null)
    {

        var created = DateTime.UtcNow.AddDays(-7);

        var task = new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: title,
            description: null,
            priority: priority,
            createdAt: created,
            dueDate: null,
            tags: tags
        );

        if (status == Status.Doing)
            task.UpdateStatus(Status.Doing);
        else if (status == Status.Done)
        {
            task.UpdateStatus(Status.Doing);
            task.UpdateStatus(Status.Done);
        }

        return task;

    }

    private void SetupTasks(params TaskItem[] tasks)
    {
        _repoMock.Setup(r => r.GetAll()).Returns(tasks.ToList());
    }

    [Fact]
    public void List_NoFilters_ReturnsAll()
    {

        var tasks = new[]
        {
            MakeTask(title: "Task jedna"),
            MakeTask(title: "Task dva"),
            MakeTask(title: "Task tři")
        };
        SetupTasks(tasks);

        var sut = CreateSut();
        var result = sut.List(null, null, null);

        result.Should().HaveCount(3);

    }

    [Fact]
    public void List_EmptyRepository_ReturnsEmpty()
    {

        SetupTasks();

        var sut = CreateSut();
        var result = sut.List(null, null, null);

        result.Should().BeEmpty();

    }

    [Fact]
    public void List_FilterByStatus_ReturnsOnlyMatching()
    {

        SetupTasks(
            MakeTask(title: "Todo task", status: Status.Todo),
            MakeTask(title: "Doing task", status: Status.Doing),
            MakeTask(title: "Done task", status: Status.Done)
        );

        var sut = CreateSut();
        var result = sut.List(Status.Doing, null, null);

        result.Should().ContainSingle()
              .Which.Title.Should().Be("Doing task");

    }

    [Fact]
    public void List_FilterByStatus_NoMatch_ReturnsEmpty()
    {

        SetupTasks(
            MakeTask(title: "Todo task", status: Status.Todo)
        );

        var sut = CreateSut();
        var result = sut.List(Status.Done, null, null);

        result.Should().BeEmpty();

    }

    [Fact]
    public void List_FilterByPriority_ReturnsOnlyMatching()
    {

        SetupTasks(
            MakeTask(title: "Low task", priority: Priority.Low),
            MakeTask(title: "High task", priority: Priority.High),
            MakeTask(title: "High task 2", priority: Priority.High)
        );

        var sut = CreateSut();
        var result = sut.List(null, Priority.High, null);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.Priority == Priority.High);

    }

    [Fact]
    public void List_FilterByTag_ReturnsOnlyMatching()
    {

        SetupTasks(
            MakeTask(title: "S tagem", tags: new[] { new Tag("csharp") }),
            MakeTask(title: "Jiný tag", tags: new[] { new Tag("python") }),
            MakeTask(title: "Bez tagu")
        );

        var sut = CreateSut();
        var result = sut.List(null, null, "csharp");

        result.Should().ContainSingle()
              .Which.Title.Should().Be("S tagem");

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void List_BlankTag_IgnoresTagFilter(string? tag)
    {

        SetupTasks(
            MakeTask(title: "Task jedna"),
            MakeTask(title: "Task dva")
        );

        var sut = CreateSut();
        var result = sut.List(null, null, tag);

        result.Should().HaveCount(2);

    }

    [Fact]
    public void List_TagWithWhitespace_IsTrimmed()
    {

        SetupTasks(
            MakeTask(title: "Tagged", tags: new[] { new Tag("blazor") })
        );

        var sut = CreateSut();
        var result = sut.List(null, null, "  blazor  ");

        result.Should().ContainSingle();

    }


    [Fact]
    public void List_StatusAndPriority_CombinesFilters()
    {

        SetupTasks(
            MakeTask(title: "Match", status: Status.Todo, priority: Priority.High),
            MakeTask(title: "Wrong status", status: Status.Doing, priority: Priority.High),
            MakeTask(title: "Wrong priority", status: Status.Todo, priority: Priority.Low)
        );

        var sut = CreateSut();
        var result = sut.List(Status.Todo, Priority.High, null);

        result.Should().ContainSingle()
              .Which.Title.Should().Be("Match");

    }

    [Fact]
    public void List_AllThreeFilters_CombinesAll()
    {

        SetupTasks(
            MakeTask(title: "Přesný match", status: Status.Doing, priority: Priority.High, tags: new[] { new Tag("urgent") }),
            MakeTask(title: "Špatný tag", status: Status.Doing, priority: Priority.High, tags: new[] { new Tag("chill") }),
            MakeTask(title: "Špatný status", status: Status.Todo, priority: Priority.High, tags: new[] { new Tag("urgent") })
        );

        var sut = CreateSut();
        var result = sut.List(Status.Doing, Priority.High, "urgent");

        result.Should().ContainSingle()
              .Which.Title.Should().Be("Přesný match");

    }

}
