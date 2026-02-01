using System;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.ConsoleApp;

public sealed class MemoryRepository : ITaskRepository
{

    private readonly Dictionary<Guid, TaskItem> Items = new();
    private readonly Dictionary<Priority, TaskItem> ItemsPriority = new();

    public void Add(TaskItem task) => Items[task.Id] = task;

    public TaskItem? GetById(Guid id) => Items.TryGetValue(id, out var task) ? task : null;

    public IReadOnlyList<TaskItem>? GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags)
    {
        
        
        IEnumerable<TaskItem> query = Items.Values;

        if (priority is not null)
            query = query.Where(t => t.Priority == priority.Value);

        if (status is not null)
            query = query.Where(t => t.Status == status.Value);

        if (tags is { Count: > 0 })
        {

            var searchedTags = tags
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (searchedTags.Length > 0)
            {

                query = query.Where(t =>
                    t.Tags is not null &&
                    t.Tags.All(tag => searchedTags.Contains(tag.Value, StringComparer.OrdinalIgnoreCase)));

            }

        }

        return query.ToList();

    }


    public IReadOnlyList<TaskItem> GetAll() => Items.Values.ToList();

    public void Save() {}

    public bool Remove(Guid id) => Items.Remove(id);


}
