using FluentAssertions;
using Moq;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Services;
using TaskBoard.Domain;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Tests.Application.Services;

/// <summary>
/// Unit testy pro <see cref="Updatetask"/> service.
/// 
/// Pokrývají:
///   - Validaci vstupů (ownerId, title, dueDate)
///   - Úspěšné úpravy jednotlivých polí
///   - Detekci "beze změny" (vrací false, nevolá Save)
///   - Kombinované úpravy více polí najednou
///   - Event TaskUpdated (vyvolání / nevyvolání)
///   - Doménové výjimky (neplatný status přechod)
/// </summary>
public sealed class UpdateTaskTests
{
    // ───────────────────────────────────────────────
    //  Společný setup — DRY příprava pro každý test
    // ───────────────────────────────────────────────

    private const string OwnerId = "owner-1";

    private readonly Mock<ITaskRepository> _repoMock = new();

    /// <summary>
    /// Vytvoří "System Under Test" — instanci Updatetask s mock repozitářem.
    /// </summary>
    private Updatetask CreateSut() => new(_repoMock.Object);

    /// <summary>
    /// Helper: vytvoří platný TaskItem, který mock repozitář vrátí při GetById.
    /// Parametry jsou volitelné — kdo je nepředá, dostane rozumné výchozí hodnoty.
    /// </summary>
    private TaskItem SetupExistingTask(
        Guid? id = null,
        string title = "Původní titulek",
        string? description = "Původní popis",
        Priority priority = Priority.Low,
        Status status = Status.Todo,
        DateTime? createdAt = null)
    {

        var taskId = id ?? Guid.NewGuid();

        var created = createdAt ?? DateTime.UtcNow.AddDays(-7);

        var task = new TaskItem(
            id: taskId,
            ownerId: OwnerId,
            title: title,
            description: description,
            priority: priority,
            createdAt: created,
            dueDate: null
        );

        if (status == Status.Doing)
            task.UpdateStatus(Status.Doing);
        else if (status == Status.Done)
        {

            task.UpdateStatus(Status.Doing);
            task.UpdateStatus(Status.Done);

        }

        _repoMock.Setup(r => r.GetById(OwnerId, taskId)).Returns(task);

        return task;

    }


    // ═══════════════════════════════════════════════
    //  1) VALIDACE VSTUPŮ
    // ═══════════════════════════════════════════════

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyOwnerId_ThrowsArgumentException(string? ownerId)
    {
        
        var sut = CreateSut();

        var act = () => sut.Update(ownerId!, Guid.NewGuid(), "Nový titulek", null, null, null, null);

        act.Should().Throw<ArgumentException>().WithParameterName("ownerId");

    }

    [Fact]
    public void Update_TaskNotFound_ThrowsKeyNotFoundException()
    {
        
        var unknownId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetById(OwnerId, unknownId)).Returns((TaskItem?)null);

        var sut = CreateSut();

        var act = () => sut.Update(OwnerId, unknownId, "Cokoliv", null, null, null, null);

        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{unknownId}*");

    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_EmptyTitle_ThrowsArgumentException(string emptyTitle)
    {
        
        SetupExistingTask();
        var taskId = _repoMock.Invocations.Count > 0 ? (Guid)_repoMock.Invocations[0].Arguments[1] : Guid.NewGuid();

        var task = SetupExistingTask();
        var sut = CreateSut();

        var act = () => sut.Update(OwnerId, task.Id, emptyTitle, null, null, null, null);

        act.Should().Throw<ArgumentException>();

    }

    [Fact]
    public void Update_TitleTooShort_ThrowsArgumentException()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();

        var act = () => sut.Update(OwnerId, task.Id, "AB", null, null, null, null);

        act.Should().Throw<ArgumentException>();

    }

    [Fact]
    public void Update_DueDateInThePast_ThrowsArgumentException()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();
        var yesterday = DateTime.Now.AddDays(-1);

        var act = () => sut.Update(OwnerId, task.Id, null, null, yesterday, null, null);

        act.Should().Throw<ArgumentException>().WithParameterName("newDueDate");

    }


    // ═══════════════════════════════════════════════
    //  2) ÚSPĚŠNÉ ÚPRAVY JEDNOTLIVÝCH POLÍ
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_NewTitle_ChangesTitle_ReturnsTrue()
    {

        var task = SetupExistingTask(title: "Starý název");
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, "Nový název", null, null, null, null);

        result.Should().BeTrue();

        task.Title.Should().Be("Nový název");

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Update_NewDescription_ChangesDescription_ReturnsTrue()
    {

        var task = SetupExistingTask(description: "Starý popis");
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, "Nový popis", null, null, null);

        result.Should().BeTrue();

        task.Description.Should().Be("Nový popis");

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Update_FutureDueDate_ChangesDueDate_ReturnsTrue()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();
        var futureDate = DateTime.Now.AddDays(30);

        var result = sut.Update(OwnerId, task.Id, null, null, futureDate, null, null);

        result.Should().BeTrue();

        task.DueDate.Should().Be(futureDate);

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Update_DifferentPriority_ChangesPriority_ReturnsTrue()
    {

        var task = SetupExistingTask(priority: Priority.Low);
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, null, null, null, Priority.High);

        result.Should().BeTrue();

        task.Priority.Should().Be(Priority.High);

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Update_ValidStatusTransition_ChangesStatus_ReturnsTrue()
    {

        var task = SetupExistingTask(status: Status.Todo);
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, null, null, Status.Doing, null);

        result.Should().BeTrue();

        task.Status.Should().Be(Status.Doing);

        _repoMock.Verify(r => r.Save(), Times.Once);

    }


    // ═══════════════════════════════════════════════
    //  3) DETEKCE "BEZE ZMĚNY" — vrací false
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_AllParametersNull_ReturnsFalse_DoesNotSave()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, null, null, null, null);

        result.Should().BeFalse();

        _repoMock.Verify(r => r.Save(), Times.Never);

    }

    [Fact]
    public void Update_SameTitle_ReturnsFalse_DoesNotSave()
    {

        var task = SetupExistingTask(title: "Beze změny");
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, "Beze změny", null, null, null, null);

        result.Should().BeFalse();

        _repoMock.Verify(r => r.Save(), Times.Never);

    }

    [Fact]
    public void Update_SamePriority_ReturnsFalse_DoesNotSave()
    {
 
        var task = SetupExistingTask(priority: Priority.Medium);
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, null, null, null, Priority.Medium);

        result.Should().BeFalse();

        _repoMock.Verify(r => r.Save(), Times.Never);

    }

    [Fact]
    public void Update_SameStatus_ReturnsFalse_DoesNotSave()
    {

        var task = SetupExistingTask(status: Status.Todo);
        var sut = CreateSut();

        var result = sut.Update(OwnerId, task.Id, null, null, null, Status.Todo, null);

        result.Should().BeFalse();

        _repoMock.Verify(r => r.Save(), Times.Never);

    }


    // ═══════════════════════════════════════════════
    //  4) DOMÉNOVÉ VÝJIMKY
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_InvalidStatusTransition_ThrowsConflictException()
    {

        var task = SetupExistingTask(status: Status.Todo);
        var sut = CreateSut();

        var act = () => sut.Update(OwnerId, task.Id, null, null, null, Status.Done, null);

        act.Should().Throw<ConflictException>();
    }


    // ═══════════════════════════════════════════════
    //  5) KOMBINOVANÉ ZMĚNY
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_MultipleFields_ChangesAll_ReturnsTrue()
    {

        var task = SetupExistingTask(
            title: "Starý",
            description: "Starý popis",
            priority: Priority.Low,
            status: Status.Todo
        );

        var sut = CreateSut();
        var newDueDate = DateTime.Now.AddDays(14);

        var result = sut.Update(
            OwnerId,
            task.Id,
            newTitle: "Úplně nový název",
            newDescription: "Nový popis",
            newDueDate: newDueDate,
            newStatus: Status.Doing,
            newPriority: Priority.High
        );

        result.Should().BeTrue();
        task.Title.Should().Be("Úplně nový název");
        task.Description.Should().Be("Nový popis");
        task.DueDate.Should().Be(newDueDate);
        task.Status.Should().Be(Status.Doing);
        task.Priority.Should().Be(Priority.High);
        _repoMock.Verify(r => r.Save(), Times.Once);

    }


    // ═══════════════════════════════════════════════
    //  6) EVENT TaskUpdated
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_WhenChanged_FiresTaskUpdatedEvent()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();

        TaskItem? eventPayload = null;
        sut.TaskUpdated += t => eventPayload = t;

        sut.Update(OwnerId, task.Id, "Změněný titulek", null, null, null, null);

        eventPayload.Should().NotBeNull();

        eventPayload!.Id.Should().Be(task.Id);

    }

    [Fact]
    public void Update_WhenNothingChanged_DoesNotFireEvent()
    {

        var task = SetupExistingTask();
        var sut = CreateSut();

        var eventFired = false;
        sut.TaskUpdated += _ => eventFired = true;

        sut.Update(OwnerId, task.Id, null, null, null, null, null);

        eventFired.Should().BeFalse();

    }


    // ═══════════════════════════════════════════════
    //  7) MarkUpdated se zavolá při změně
    // ═══════════════════════════════════════════════

    [Fact]
    public void Update_WhenChanged_SetsUpdatedAt()
    {

        var task = SetupExistingTask();
        task.UpdatedAt.Should().BeNull("protože jsme ho právě vytvořili");

        var sut = CreateSut();
        var before = DateTime.UtcNow;

        sut.Update(OwnerId, task.Id, "Aktualizovaný", null, null, null, null);

        task.UpdatedAt.Should().NotBeNull();
        
        task.UpdatedAt!.Value.Should().BeOnOrAfter(before);

    }
}
