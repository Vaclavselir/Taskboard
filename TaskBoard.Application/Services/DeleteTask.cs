using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Application.Services;

public sealed class DeleteTask
{
    private readonly ITaskRepository _repo;

    public event Action<TaskItem>? TaskDeleted;

    public DeleteTask(ITaskRepository repo)
    {
        
        _repo = repo;

    }


    public void Delete(Guid id)
    {
        
        var taskItem = _repo.GetById(id) ?? throw new KeyNotFoundException($"Task {id} not found.");

        _repo.Remove(id);
        _repo.Save();


        TaskDeleted?.Invoke(taskItem);

    }

}
