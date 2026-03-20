using System;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class EFRepository : ITaskRepository, IUserRepository
{

    private readonly  TaskBoardDbContext _db;

    public EFRepository(TaskBoardDbContext db) => _db = db;

    public void Add(TaskItem task) => _db.Tasks.Add(task);

    public TaskItem? GetById(string ownerId,Guid id)
        => _db.Tasks.FirstOrDefault(t => t.OwnerId == ownerId && t.Id == id);

    public IReadOnlyList<TaskItem> GetAll()
        => _db.Tasks.AsNoTracking().ToList();

    public Paged<TaskItem> GetByTask(string ownerId, Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize)
    {
        
        var query = _db.Tasks.AsNoTracking().Where(t => t.OwnerId == ownerId);

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

    public bool Remove(string ownerId, Guid id)
    {
        
        var entity = _db.Tasks.FirstOrDefault(t => t.OwnerId == ownerId && t.Id == id);

        if(entity is null) return false;

        _db.Tasks.Remove(entity);

        return true;

    }

    public void Save() => _db.SaveChanges();

    // IUser
    public void Add(User user) => _db.Users.Add(user);

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {

        var normalizedEmail = NormalizeEmail(email);

        return _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {

        var normalizedEmail = NormalizeEmail(email);

        return _db.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

    }

    public Task SaveAsync(CancellationToken cancellationToken = default) => _db.SaveChangesAsync(cancellationToken);

    private static string NormalizeEmail(string email)
    {

        email = (email ?? string.Empty).Trim();

        if (email.Length == 0)
            return string.Empty;

        return email.ToUpperInvariant();

    }

}
