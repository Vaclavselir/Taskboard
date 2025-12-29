using System;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.ConsoleApp;

public sealed class MemoryRepository : ITaskRepository
{

    private readonly Dictionary<Guid, TaskItem> Items = new();

    public void Add(TaskItem task) => Items[task.Id] = task;

    public TaskItem? GetById(Guid id) => Items.TryGetValue(id, out var task) ? task : null;

    public IReadOnlyList<TaskItem> GetAll() => Items.Values.ToList();

    public void Save() {}

    public bool Remove(Guid id) => Items.Remove(id);


}
