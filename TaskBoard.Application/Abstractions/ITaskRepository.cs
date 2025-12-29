using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public interface ITaskRepository
{   

    void Add(TaskItem task);

    TaskItem? GetById(Guid id);

    IReadOnlyList<TaskItem> GetAll();

    void Save();

    bool Remove(Guid id);

}
