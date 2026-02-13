using System;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class EFRepository : ITaskRepository
{

    private readonly  TaskBoardDbContext _db;

    public EFRepository(TaskBoardDbContext db) => _db = db;

    public void Add(TaskItem task) => _db.Tasks.Add(task);

    public TaskItem? GetById(Guid id)
        => _db.Tasks.FirstOrDefault(t => t.Id == id);


    public IReadOnlyList<TaskItem> GetAll()
        => _db.Tasks.AsNoTracking().ToList();


    public Paged<TaskItem> GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize)
    {
        
        var query = _db.Tasks.AsNoTracking().AsQueryable();

        if (priority is not null)
            query = query.Where(t => t.Priority == priority.Value);

        if (status is not null)
            query = query.Where(t => t.Status == status.Value);

        if (tags is { Count: > 0 })
        {

            foreach (var tag in tags)
                query = query.Where(t => t.Tags.Any(x => x.Value == tag));

        }

        
        var total = query.Count();

        var items = query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new Paged<TaskItem>(items, total);

    }

    public bool Remove(Guid id)
    {
        
        var entity = _db.Tasks.FirstOrDefault(t => t.Id == id);

        if(entity is null) return false;

        _db.Tasks.Remove(entity);

        return true;

    }

    public void Save() => _db.SaveChanges();

}
