using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Application.Services;

public sealed class ChangePriority
{

    private readonly ITaskRepository _repo;

    public event Action<Guid, Priority, Priority>? PriorityChanged;

    public ChangePriority(ITaskRepository repo)
    {
        
        _repo = repo;

    }


    public void ChangePri(Guid id, Priority newPriority)
    {
        

        var taskItem = _repo.GetById(id) ?? throw new KeyNotFoundException($"Task {id} not found.");

        var old = taskItem.Priority;

        taskItem.UpdatePriority(newPriority);

        _repo.Save();

        PriorityChanged?.Invoke(id, old, newPriority);


    }



}
