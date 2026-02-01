using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public sealed record Paged<T>(IReadOnlyList<T> Items, int TotalCount);

public interface ITaskRepository
{   

    void Add(TaskItem task);

    TaskItem? GetById(Guid id);

    Paged<TaskItem> GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize);
    
    IReadOnlyList<TaskItem> GetAll();

    void Save();

    bool Remove(Guid id);

}
