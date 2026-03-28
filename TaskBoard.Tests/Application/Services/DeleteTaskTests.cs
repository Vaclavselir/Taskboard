using FluentAssertions;
using Moq;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Services;
using TaskBoard.Domain;

namespace TaskBoard.Tests.Application.Services;

public class DeleteTaskTests
{

        private const string OwnerId = "owner-1";

    private readonly Mock<ITaskRepository> _repoMock = new();

    private DeleteTask CreateSut() => new(_repoMock.Object);

    private TaskItem SetupExistingTask(Guid? id = null)
    {

        var taskId = id ?? Guid.NewGuid();

        var task = new TaskItem(
            id: taskId,
            ownerId: OwnerId,
            title: "Task k smazání",
            description: null,
            priority: Priority.Low,
            createdAt: DateTime.UtcNow.AddDays(-1),
            dueDate: null
        );

        _repoMock.Setup(r => r.GetById(OwnerId, taskId)).Returns(task);

        return task;

    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Delete_EmptyOwnerId_ThrowsArgumentException(string? ownerId)
    {

        var sut = CreateSut();

        var act = () => sut.Delete(ownerId!, Guid.NewGuid());

        act.Should().Throw<ArgumentException>().WithParameterName("ownerId");

    }

    [Fact]
    public void Delete_TaskNotFound_ThrowsKeyNotFoundException()
    {

        var unknownId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(OwnerId, unknownId)).Returns((TaskItem?)null);

        var sut = CreateSut();

        var act = () => sut.Delete(OwnerId, unknownId);

        act.Should().Throw<KeyNotFoundException>();

    }


    [Fact]
    public void Delete_ExistingTask_CallsRemoveAndSave()
    {

        var task = SetupExistingTask();

        var sut = CreateSut();
        sut.Delete(OwnerId, task.Id);

        _repoMock.Verify(r => r.Remove(OwnerId, task.Id), Times.Once);
        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Delete_ExistingTask_RemoveIsCalledBeforeSave()
    {

        var task = SetupExistingTask();
        var callOrder = new List<string>();

        _repoMock.Setup(r => r.Remove(OwnerId, task.Id)).Callback(() => callOrder.Add("Remove"));
        _repoMock.Setup(r => r.Save()).Callback(() => callOrder.Add("Save"));

        var sut = CreateSut();
        sut.Delete(OwnerId, task.Id);

        callOrder.Should().ContainInOrder("Remove", "Save");

    }

    [Fact]
    public void Delete_ExistingTask_FiresTaskDeletedEvent()
    {

        var task = SetupExistingTask();
        TaskItem? eventArg = null;

        var sut = CreateSut();
        sut.TaskDeleted += t => eventArg = t;

        sut.Delete(OwnerId, task.Id);

        eventArg.Should().NotBeNull();
        eventArg!.Id.Should().Be(task.Id);

    }

    [Fact]
    public void Delete_NoSubscriber_DoesNotThrow()
    {

        var task = SetupExistingTask();

        var sut = CreateSut();

        var act = () => sut.Delete(OwnerId, task.Id);

        act.Should().NotThrow();

    }

    [Fact]
    public void Delete_TaskNotFound_DoesNotFireEvent()
    {

        var unknownId = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(OwnerId, unknownId)).Returns((TaskItem?)null);

        bool eventFired = false;

        var sut = CreateSut();
        sut.TaskDeleted += _ => eventFired = true;

        var act = () => sut.Delete(OwnerId, unknownId);

        act.Should().Throw<KeyNotFoundException>();
        eventFired.Should().BeFalse();

    }

}
