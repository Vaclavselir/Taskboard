using FluentAssertions;
using Moq;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Services;
using TaskBoard.Domain;
using TaskBoard.Domain.Exceptions;

namespace TaskBoard.Tests.Application.Services;

public sealed class CreateTaskTest
{

    private const string OwnerId = "owner-1";

    private readonly Mock<ITaskRepository> _repoMock = new();

    private readonly Mock<ITime> _timeMock  = new();

    private readonly Mock<IGeneratorId> _idsMock  = new();

    public CreateTaskTest()
    {
        _idsMock.Setup(g => g.NewGuid()).Returns(Guid.NewGuid());
    }

    private CreateTask CreateSut() => new(_repoMock.Object, _timeMock.Object, _idsMock.Object);

    TaskCommand cmd = new TaskCommand(Title: "Naučit se testy", Description: "A přežít pohovor", Priority: Priority.High, DueDate: DateTime.Now.AddDays(7), Tags: new[] { "csharp", "testing" });

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOwnerId_ThrowsArgumentException(string? ownerId)
    {
        
        var sut = CreateSut();

        var act = () => sut.Create(ownerId!, cmd);

        act.Should().Throw<ArgumentException>().WithParameterName("ownerId");

    }

    [Theory]
    [InlineData(null)]
    public void Create_EmptyCmd_ThrowsArgumentException(TaskCommand cmd)
    {
        
        var sut = CreateSut();

        var act = () => sut.Create(OwnerId, cmd!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("cmd");

    }

    [Fact]
    public void Create_ValidInput_ReturnsGeneratedId()
    {
        
        var expectedId = Guid.NewGuid();
        _idsMock.Setup(g => g.NewGuid()).Returns(expectedId);

        var sut = CreateSut();

        var result = sut.Create(OwnerId, cmd);

        result.Should().Be(expectedId); 

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Create_ValidInput_SetsCorrectCreatedAt()
    {
        
        var fixedTime = new DateTime(2025, 1, 1, 12, 0, 0); 
        _timeMock.Setup(t => t.Now).Returns(fixedTime);

        TaskItem? captured = null;

        _repoMock.Setup(r => r.Add(It.IsAny<TaskItem>())).Callback<TaskItem>(t => captured = t);

        var sut = CreateSut();
        sut.Create(OwnerId, cmd);

        captured.Should().NotBeNull();
        captured!.CreatedAt.Should().Be(fixedTime);

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

    [Fact]
    public void Create_ValidInput_SetsCorrectPriority()
    {

        TaskItem? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<TaskItem>())).Callback<TaskItem>(t => captured = t);

        var sut = CreateSut();
        sut.Create(OwnerId, cmd);

        captured.Should().NotBeNull();
        captured!.Priority.Should().Be(Priority.High);

    }

    [Fact]
    public void Create_ValidInput_SetsCorrectTags()
    {

        TaskItem? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<TaskItem>())).Callback<TaskItem>(t => captured = t);

        var sut = CreateSut();
        sut.Create(OwnerId, cmd);

        captured.Should().NotBeNull();
        captured!.Tags.Select(t => t.Value).Should().BeEquivalentTo("csharp", "testing");

    }

    [Fact]
    public void Create_ValidInput_StatusIsTodo()
    {

        TaskItem? captured = null;
        _repoMock.Setup(r => r.Add(It.IsAny<TaskItem>())).Callback<TaskItem>(t => captured = t);

        var sut = CreateSut();
        sut.Create(OwnerId, cmd);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(Status.Todo);

    }

    [Fact]
    public void Create_TitleTooShort_ThrowsArgumentException()
    {

        var badCmd = new TaskCommand(
            Title: "AB",
            Description: null,
            Priority: Priority.Low,
            DueDate: null,
            Tags: null
        );

        var sut = CreateSut();

        var act = () => sut.Create(OwnerId, badCmd);

        act.Should().Throw<ArgumentException>();

    }

}
