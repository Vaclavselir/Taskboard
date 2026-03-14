using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Application.Services;

public sealed class CreateTask
{

    private readonly ITaskRepository _repo;

    private readonly ITime _time;

    private readonly IGeneratorId _ids;

    public event Action<TaskItem>? TaskCreated;


    public CreateTask(ITaskRepository repo, ITime time, IGeneratorId ids)
    {
        
        _repo = repo;
        _time = time;
        _ids = ids;

    }

    public Guid Create(string ownerId, TaskCommand cmd)
    {
        
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        if (cmd is null) throw new ArgumentNullException(nameof(cmd));
        

        var tags = (cmd.Tags ?? Array.Empty<string>())
            .Select(t => new Tag(t))
            .Distinct()
            .ToList();

        var task = new TaskItem(

            id: _ids.NewGuid(),
            ownerId: ownerId,
            title: cmd.Title,
            description: cmd.Description,
            priority: cmd.Priority,
            createdAt: _time.Now,
            dueDate: cmd.DueDate,
            tags: tags

        );

        _repo.Add(task);
        _repo.Save();

        TaskCreated?.Invoke(task);

        return task.Id;

    }


}
