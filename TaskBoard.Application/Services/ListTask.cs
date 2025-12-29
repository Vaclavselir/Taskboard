using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Application.Services;

public sealed class ListTask
{

    public readonly ITaskRepository _repo;

    public ListTask(ITaskRepository repo)
    {
        
        _repo = repo;

    }


    public IReadOnlyList<TaskItem> List(Status? status, Priority? priority, string? tag)
    {
        
        var tasks = _repo.GetAll().AsEnumerable();

        if (status is not null)
            tasks = tasks.Where(t => t.Status == status.Value);

        if (priority is not null)
            tasks = tasks.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(tag))
            tasks = tasks.Where(t => t.Tags.Any(x => x.Value.Contains(tag.Trim())));

        return tasks.ToList();

    }




}
