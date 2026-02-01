using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public interface ITaskRepository
{   

    void Add(TaskItem task);

    TaskItem? GetById(Guid id);

    IReadOnlyList<TaskItem>? GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags);
    
    IReadOnlyList<TaskItem> GetAll();

    void Save();

    bool Remove(Guid id);

}
