using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

/// <summary>
/// Generic paged container used by repositories for list endpoints.
/// </summary>
/// <typeparam name="T">Item type (domain entity / DTO, depending on layer).</typeparam>
/// <param name="Items">
/// Items for the requested page (already paged). Never null, can be empty.
/// </param>
/// <param name="TotalCount">
/// Total number of items matching the filter BEFORE paging (so UI can show pages).
/// </param>
public sealed record Paged<T>(IReadOnlyList<T> Items, int TotalCount);

public interface ITaskRepository
{   

    /// <summary>
    /// Adds a new task into the repository (in-memory / tracked state).
    /// NOTE: persistence is not guaranteed until <see cref="Save"/> is called.
    /// </summary>
    /// <param name="task">Task to add. Must not be null.</param>
    void Add(TaskItem task);

    /// <summary>
    /// Finds a task by its identifier.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <returns>
    /// The task instance if found; otherwise null.
    /// </returns>
    TaskItem? GetById(Guid id);

    /// <summary>
    /// Returns tasks matching the provided optional filters, with paging applied.
    /// 
    /// Contract:
    /// - priority/status/tags are OPTIONAL: null means "do not filter by this criterion".
    /// - tags filtering is AND-based (task must contain ALL provided tags), unless implementation states otherwise.
    /// - Items is never null (empty list when nothing matches).
    /// 
    /// </summary>
    Paged<TaskItem> GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize);
    
    IReadOnlyList<TaskItem> GetAll();

    void Save();

    /// <summary>
    /// Removes task with the given id (if present).
    /// NOTE: persistence is not guaranteed until <see cref="Save"/> is called.
    /// </summary>
    /// <param name="id">Task identifier.</param>
    /// <returns>
    /// True if the task existed and was removed; otherwise false.
    /// </returns>
    bool Remove(Guid id);

}
