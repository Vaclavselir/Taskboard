using FluentAssertions;
using TaskBoard.Domain;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Tests.Application.Services;

public class TaskItemTests
{

    private const string OwnerId = "owner-1";

    private static TaskItem CreateTask(
        string title = "Validní titulek",
        string? description = "Popis",
        Priority priority = Priority.Low,
        DateTime? createdAt = null,
        DateTime? dueDate = null)
    {

        var created = createdAt ?? DateTime.UtcNow.AddDays(-7);

        return new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: title,
            description: description,
            priority: priority,
            createdAt: created,
            dueDate: dueDate
        );

    }


    // ═══════════════════════════════════════════════
    //  1) KONSTRUKTOR — validace
    // ═══════════════════════════════════════════════

    [Fact]
    public void Ctor_EmptyId_ThrowsArgumentException()
    {

        var act = () => new TaskItem(
            id: Guid.Empty,
            ownerId: OwnerId,
            title: "Test",
            description: null,
            priority: Priority.Low,
            createdAt: DateTime.UtcNow,
            dueDate: null
        );

        act.Should().Throw<ArgumentException>().WithParameterName("id");

    }

    [Theory]
    [InlineData("AB")]
    [InlineData("  AB  ")]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_TitleTooShort_ThrowsArgumentException(string title)
    {

        var act = () => CreateTask(title: title);

        act.Should().Throw<ArgumentException>().WithParameterName("title");

    }

    [Fact]
    public void Ctor_DueDateBeforeCreatedAt_ThrowsArgumentException()
    {

        var created = DateTime.UtcNow;

        var act = () => new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: "Test task",
            description: null,
            priority: Priority.Low,
            createdAt: created,
            dueDate: created.AddDays(-1)
        );

        act.Should().Throw<ArgumentException>().WithParameterName("dueDate");

    }

    [Fact]
    public void Ctor_ValidInput_SetsDefaultStatus()
    {

        var task = CreateTask();

        task.Status.Should().Be(Status.Todo);

    }

    [Fact]
    public void Ctor_WhitespaceDescription_NormalizesToNull()
    {

        var task = CreateTask(description: "   ");

        task.Description.Should().BeNull();

    }

    [Fact]
    public void Ctor_TitleWithSpaces_IsTrimmed()
    {

        var task = CreateTask(title: "  Trimovaný titulek  ");

        task.Title.Should().Be("Trimovaný titulek");

    }


    // ═══════════════════════════════════════════════
    //  2) STATUS TRANSITIONS
    // ═══════════════════════════════════════════════

    [Fact]
    public void UpdateStatus_TodoToDoing_Succeeds()
    {

        var task = CreateTask();

        task.UpdateStatus(Status.Doing);

        task.Status.Should().Be(Status.Doing);

    }

    [Fact]
    public void UpdateStatus_DoingToDone_Succeeds()
    {

        var task = CreateTask();
        task.UpdateStatus(Status.Doing);

        task.UpdateStatus(Status.Done);

        task.Status.Should().Be(Status.Done);

    }

    [Fact]
    public void UpdateStatus_TodoToDone_ThrowsConflictException()
    {

        var task = CreateTask();

        var act = () => task.UpdateStatus(Status.Done);

        act.Should().Throw<ConflictException>();

    }

    [Fact]
    public void UpdateStatus_DoingToTodo_ThrowsConflictException()
    {

        var task = CreateTask();
        task.UpdateStatus(Status.Doing);

        var act = () => task.UpdateStatus(Status.Todo);

        act.Should().Throw<ConflictException>();

    }

    [Fact]
    public void UpdateStatus_SameStatus_NoChange()
    {

        var task = CreateTask();

        task.UpdateStatus(Status.Todo); // Todo → Todo

        task.Status.Should().Be(Status.Todo);

    }


    // ═══════════════════════════════════════════════
    //  3) IsOverdue COMPUTED PROPERTY
    // ═══════════════════════════════════════════════

    [Fact]
    public void IsOverdue_PastDueDateAndNotDone_ReturnsTrue()
    {

        var created = DateTime.UtcNow.AddDays(-10);

        var task = new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: "Overdue task",
            description: null,
            priority: Priority.High,
            createdAt: created,
            dueDate: created.AddDays(1) // dueDate je 9 dní v minulosti
        );

        task.IsOverdue.Should().BeTrue();

    }

    [Fact]
    public void IsOverdue_PastDueDateButDone_ReturnsFalse()
    {

        var created = DateTime.UtcNow.AddDays(-10);

        var task = new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: "Hotový task",
            description: null,
            priority: Priority.High,
            createdAt: created,
            dueDate: created.AddDays(1)
        );

        task.UpdateStatus(Status.Doing);
        task.UpdateStatus(Status.Done);

        task.IsOverdue.Should().BeFalse();

    }

    [Fact]
    public void IsOverdue_NoDueDate_ReturnsFalse()
    {

        var task = CreateTask(dueDate: null);

        task.IsOverdue.Should().BeFalse();

    }

    [Fact]
    public void IsOverdue_FutureDueDate_ReturnsFalse()
    {

        var created = DateTime.UtcNow.AddDays(-1);

        var task = new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: "Budoucí task",
            description: null,
            priority: Priority.Low,
            createdAt: created,
            dueDate: DateTime.UtcNow.AddDays(30)
        );

        task.IsOverdue.Should().BeFalse();

    }


    // ═══════════════════════════════════════════════
    //  4) UPDATE METODY
    // ═══════════════════════════════════════════════

    [Fact]
    public void UpdateTitle_ValidTitle_ChangesTitle()
    {

        var task = CreateTask();

        task.UpdateTitle("Nový titulek");

        task.Title.Should().Be("Nový titulek");

    }

    [Theory]
    [InlineData("AB")]
    [InlineData("")]
    public void UpdateTitle_TooShort_ThrowsArgumentException(string title)
    {

        var task = CreateTask();

        var act = () => task.UpdateTitle(title);

        act.Should().Throw<ArgumentException>();

    }

    [Fact]
    public void UpdateDueDate_BeforeCreatedAt_ThrowsArgumentException()
    {

        var task = CreateTask();

        var act = () => task.UpdateDueDate(task.CreatedAt.AddDays(-1));

        act.Should().Throw<ArgumentException>();

    }

    [Fact]
    public void UpdateDueDate_Null_ClearsDueDate()
    {

        var created = DateTime.UtcNow.AddDays(-7);
        var task = new TaskItem(
            id: Guid.NewGuid(),
            ownerId: OwnerId,
            title: "Task s due date",
            description: null,
            priority: Priority.Low,
            createdAt: created,
            dueDate: created.AddDays(14)
        );

        task.UpdateDueDate(null);

        task.DueDate.Should().BeNull();

    }


    // ═══════════════════════════════════════════════
    //  5) TAGY
    // ═══════════════════════════════════════════════

    [Fact]
    public void AddTag_NewTag_IsAdded()
    {

        var task = CreateTask();

        task.AddTag(new Tag("urgent"));

        task.Tags.Should().ContainSingle(t => t.Value == "urgent");

    }

    [Fact]
    public void AddTag_Duplicate_IsNotAddedTwice()
    {

        var task = CreateTask();
        task.AddTag(new Tag("urgent"));

        task.AddTag(new Tag("urgent"));

        task.Tags.Should().HaveCount(1);

    }

    [Fact]
    public void RemoveTag_ExistingTag_IsRemoved()
    {

        var task = CreateTask();
        var tag = new Tag("remove-me");
        task.AddTag(tag);

        task.RemoveTag(tag);

        task.Tags.Should().BeEmpty();

    }

}
