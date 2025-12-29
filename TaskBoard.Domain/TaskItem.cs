using System;
using System.Windows.Markup;

namespace TaskBoard.Domain;

public sealed class TaskItem
{
    public Guid Id {get;}

    public string Title {get; private set;}

    public string? Description { get; private set; }

    public Status Status {get; private set;}

    public Priority Priority { get; private set; }

    public DateTime CreatedAt {get;}

    public DateTime? DueDate {get; private set;}

    public List<Tag> Tags {get;}


    public TaskItem(Guid id, string title, string? description, Priority priority, DateTime createdAt, DateTime? dueDate, IEnumerable<Tag>? tags = null)
    {
        
        if (id == Guid.Empty) throw new ArgumentException("Id cannot be empty", nameof(id));
        
        title = (title ?? string.Empty).Trim();

        if (title.Length < 3) throw new ArgumentException("Title must be at least 3 characters.", nameof(title));

        if (dueDate is not null && dueDate < createdAt) throw new ArgumentException("DueDate cannot be earlier than CreatedAt.", nameof(dueDate));
           
        
        Id = id;
        Title = title;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Priority = priority;
        CreatedAt = createdAt;
        DueDate = dueDate;
        Status = Status.Todo;
        Tags = tags?.Distinct().ToList() ?? new List<Tag>();


    }


    public void ChangeStatus(Status newStatus)
    {
        
        if (newStatus == Status) return;

        var isValid =
            (Status == Status.Todo && newStatus == Status.Doing) ||
            (Status == Status.Doing && newStatus == Status.Done);

        if (!isValid)
            throw new InvalidOperationException($"Invalid status transition: {Status} -> {newStatus}");

        Status = newStatus;

    }

    public void UpdateTitle(string title)
    {
        title = (title ?? string.Empty).Trim();
        if (title.Length < 3) throw new ArgumentException("Title must be at least 3 characters.", nameof(title));
        Title = title;
    }


    public void UpdateDescription(string? description) => Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    public void UpdatePriority(Priority priority) => Priority = priority;

    public void UpdateDueDate(DateTime? dueDate)
    {

        if (dueDate is not null && dueDate < CreatedAt)
            throw new ArgumentException("DueDate cannot be earlier thanFnd CreatedAt.", nameof(dueDate));

        DueDate = dueDate;

    }

    public void AddTag(Tag tag)
    {

        if (!Tags.Contains(tag))
            Tags.Add(tag);

    }

    public void RemoveTag(Tag tag) => Tags.Remove(tag);



}
