using System;

namespace TaskBoard.Infrastructure.Persistence;

using System.Text.Json;
using System.Text.Json.Serialization;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

public sealed class JsonRepository : ITaskRepository
{

    private readonly string _filePath;
    private readonly Dictionary<Guid, TaskItem> _items;
    
    private readonly object _gate = new();


    private static readonly JsonSerializerOptions JsonOptions = new()
    {

        WriteIndented = true,
        Converters = {new JsonStringEnumConverter()}

    };


    public JsonRepository(string filePath)
    {

        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _items = LoadFromDisk(_filePath);

    }


    public void Add(TaskItem task)
    {

        lock (_gate)
        {
            
            _items[task.Id] = task;

        }

    }

    public TaskItem? GetById(Guid id)
    {

        lock (_gate)
        {

            return _items.TryGetValue(id, out var task) ? task : null;

        }

    }

    public Paged<TaskItem> GetByTask(Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize)
    {
        
        lock (_gate)
        {

            IEnumerable<TaskItem> query = _items.Values;

            if (priority is not null)
                query = query.Where(t => t.Priority == priority.Value);

            if (status is not null)
                query = query.Where(t => t.Status == status.Value);

            if (tags is { Count: > 0 })
            {

                var searchedTags = tags
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => new Tag(x).Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (searchedTags.Length > 0)
                {

                    query = query.Where(t =>
                        t.Tags is not null &&
                            searchedTags.All(st =>
                                t.Tags.Any(tag => string.Equals(tag.Value, st, StringComparison.OrdinalIgnoreCase))
                        ));
                    
                }

            }

            var filtered = query.ToList();
            var total = filtered.Count;

            var items = filtered
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new Paged<TaskItem>(items, total);
            
        }

    }

    public IReadOnlyList<TaskItem> GetAll()
    {

        lock (_gate)
            return _items.Values.ToList();

    }


    public bool Remove(Guid id)
    {

        lock (_gate)
            return _items.Remove(id);

    }


    public void Save()
    {

        lock (_gate)
        {

            var records = _items.Values.Select(MapFromDomain).ToList();
            WriteJson(records);

        }

    }


    private static Dictionary<Guid, TaskItem> LoadFromDisk(string filePath)
    {

        if (!File.Exists(filePath))
            return new Dictionary<Guid, TaskItem>();


        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<Guid, TaskItem>();


        var records = JsonSerializer.Deserialize<List<TaskRecord>>(json, JsonOptions) ?? new List<TaskRecord>();

        var dict = new Dictionary<Guid, TaskItem>(capacity: records.Count);
        foreach (var r in records)
        {
            var task = MapToDomain(r);
            dict[task.Id] = task;
        }

        return dict;

    }


    private void WriteJson(List<TaskRecord> records)
    {

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var tmp = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(records, JsonOptions);

        File.WriteAllText(tmp, json);

        if (File.Exists(_filePath))
            File.Replace(tmp, _filePath, destinationBackupFileName: null, ignoreMetadataErrors: true);
        else
            File.Move(tmp, _filePath);
            
    }

    

    private static TaskRecord MapFromDomain(TaskItem t) => new()
    {

        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        DueDate = t.DueDate,
        Tags = t.Tags.Select(x => x.Value).ToList()

    };


    private static TaskItem MapToDomain(TaskRecord r)
    {

        var tags = (r.Tags ?? new List<string>())
            .Select(x => new Tag(x))
            .Distinct()
            .ToList();

        
        var task = new TaskItem(

            id: r.Id,
            title: r.Title,
            description: r.Description,
            priority: r.Priority,
            createdAt: r.CreatedAt,
            dueDate: r.DueDate,
            tags: tags

        );

        if (r.Status == Status.Doing)
        {

            task.UpdateStatus(Status.Doing);

        }
        else if (r.Status == Status.Done)
        {

            task.UpdateStatus(Status.Doing);
            task.UpdateStatus(Status.Done);

        }

        return task;
        
    }

}
