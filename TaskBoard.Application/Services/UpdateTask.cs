using System;
using System.ComponentModel;
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


    public bool Update(string ownerId, Guid id, string? newTitle, string? newDescription, DateTime? newDueDate, Status? newStatus, Priority? newPriority)
    {
        
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));

        var taskItem = _repo.GetById(ownerId, id) ?? throw new KeyNotFoundException($"Task {id} not found.");

        var changed = false;

        if (newTitle is not null)
        {

            var t = newTitle.Trim();

            if (t.Length == 0) throw new ArgumentException("Title cannot be empty.", nameof(newTitle));

            if (!string.Equals(taskItem.Title, t, StringComparison.Ordinal))
            {

                taskItem.UpdateTitle(t);
                changed = true;

            }

        }

        if (newDescription is not null)
        {

            taskItem.UpdateDescription(newDescription);

            changed = true;

        }
            

        if (newDueDate is not null)
        {

            var now = DateTime.Now;

            if(newDueDate <= now) throw new ArgumentException("DueDate cannot be in the past.", nameof(newDueDate));

            taskItem.UpdateDueDate(newDueDate);

            changed = true;

        }
            

        if (newPriority is not null)
        {

            var oldPriority = taskItem.Priority;

            taskItem.UpdatePriority(newPriority.Value);

            if(taskItem.Priority != oldPriority) 
                 changed = true;

        }

        if (newStatus is not null)
        {

            var oldStatus= taskItem.Status;

            taskItem.UpdateStatus(newStatus.Value);

            if(taskItem.Status != oldStatus) 
                 changed = true;

        }

        if (!changed) return false;
        

        _repo.Save();
        TaskUpdated?.Invoke(taskItem);

        return true;


    }


}
