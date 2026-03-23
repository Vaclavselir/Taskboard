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

        captured.Should().NotBeNull();
        captured!.CreatedAt.Should().Be(fixedTime);

        _repoMock.Verify(r => r.Save(), Times.Once);

    }

}
