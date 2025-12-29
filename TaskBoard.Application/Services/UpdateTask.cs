using System;
using System.Reflection;
using Microsoft.VisualBasic;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;
namespace TaskBoard.Application.Services;

public sealed class Updatetask
{

    private readonly ITaskRepository _repo;

    public event Action<TaskItem>? TaskUpdated;

    public Updatetask(ITaskRepository repo)
    {

        _repo = repo;

    }


    public void Update(Guid id, string? newTitle, string? newDescription, DateTime? newDueDate)
    {
        
        
        var taskItem = _repo.GetById(id) ?? throw new KeyNotFoundException($"Task {id} not found.");

        var oldTitle = taskItem.Title;

        if (!string.IsNullOrWhiteSpace(newTitle))
            taskItem.UpdateTitle(newTitle);

        if (newDescription is not null)
            taskItem.UpdateDescription(newDescription);

        if (newDueDate is not null)
            taskItem.UpdateDueDate(newDueDate);

            
        _repo.Save();

        TaskUpdated?.Invoke(taskItem);


    }


}
