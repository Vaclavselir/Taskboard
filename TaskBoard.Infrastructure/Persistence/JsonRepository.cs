using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;
using TaskBoard.Infrastructure.Persistence.Entities;

namespace TaskBoard.Infrastructure.Persistence;

public sealed class JsonRepository : ITaskRepository, IUserRepository
{

    private readonly string _filePath;
    private readonly string _usersFilePath;
    private readonly Dictionary<Guid, TaskItem> _items;
    private readonly Dictionary<string, User> _users;
    
    private readonly object _gate = new();


    private static readonly JsonSerializerOptions JsonOptions = new()
    {

        WriteIndented = true,
        Converters = {new JsonStringEnumConverter()}

    };


    public JsonRepository(string filePath)
    {

        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _usersFilePath = BuildUsersFilePath(_filePath);

        _items = LoadTaskFromDisk(_filePath);
        _users = LoadUsersFromDisk(_usersFilePath);

    }


    public void Add(TaskItem task)
    {

        lock (_gate)
        {
            
            _items[task.Id] = task;

        }

    }

    public TaskItem? GetById(string ownerId, Guid id)
    {

        lock (_gate)
        {

            if (!_items.TryGetValue(id, out var task))
                return null;

            return task.OwnerId == ownerId ? task : null;

        }

    }

    public Paged<TaskItem> GetByTask(string ownerId, Priority? priority, Status? status, IReadOnlyCollection<string>? tags, int pageNumber, int pageSize)
    {
        
        lock (_gate)
        {

            IEnumerable<TaskItem> query = _items.Values.Where(t => t.OwnerId == ownerId);

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
                            t.Tags.Any(tag => string.Equals(tag.Value, st, StringComparison.OrdinalIgnoreCase))));

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


    public bool Remove(string ownerId, Guid id)
    {

        lock (_gate)
        {
            if (!_items.TryGetValue(id, out var task))
                return false;

            if (task.OwnerId != ownerId)
                return false;

            return _items.Remove(id);
        }

    }

    public void Add(User user)
    {

        lock (_gate)
        {

            _users[user.Id] = user;
            
        }

    }

    public User? GetById(string id)
    {
        lock (_gate)
        {
            return _users.TryGetValue(id, out var user) ? user : null;
        }
    }

    public User? GetByEmail(string email)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (_gate)
        {
            return _users.Values.FirstOrDefault(u => u.Email == normalizedEmail);
        }
    }

    public bool ExistsByEmail(string email)
    {
        var normalizedEmail = NormalizeEmail(email);

        lock (_gate)
        {
            return _users.Values.Any(u => u.Email == normalizedEmail);
        }
    }

    public void Save()
    {

        lock (_gate)
        {

            var taskRecords = _items.Values.Select(MapTaskFromDomain).ToList();
            WriteJson(_filePath, taskRecords);

            var userRecords = _users.Values.Select(MapUserFromDomain).ToList();
            WriteJson(_usersFilePath, userRecords);

        }

    }

    private static Dictionary<Guid, TaskItem> LoadTaskFromDisk(string filePath)
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
            var task = MapTaskToDomain(r);
            dict[task.Id] = task;
        }

        return dict;

    }

    private static TaskRecord MapTaskFromDomain(TaskItem t) => new()
    {

        Id = t.Id,
        OwnerId = t.OwnerId,
        Title = t.Title,
        Description = t.Description,
        Status = t.Status,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        DueDate = t.DueDate,
        Tags = t.Tags.Select(x => x.Value).ToList()

    };


    private static TaskItem MapTaskToDomain(TaskRecord r)
    {

        var tags = (r.Tags ?? new List<string>())
            .Select(x => new Tag(x))
            .Distinct()
            .ToList();

        
        var task = new TaskItem(

            id: r.Id,
            ownerId: r.OwnerId,
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


    private static Dictionary<string, User> LoadUsersFromDisk(string filePath)
    {

        if (!File.Exists(filePath))
            return new Dictionary<string, User>(StringComparer.Ordinal);

        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, User>(StringComparer.Ordinal);

        var records = JsonSerializer.Deserialize<List<UserRecord>>(json, JsonOptions) ?? new List<UserRecord>();

        var dict = new Dictionary<string, User>(records.Count, StringComparer.Ordinal);

        foreach (var r in records)
        {

            var user = MapUserToDomain(r);
            dict[user.Id] = user;

        }

        return dict;

    }

    private static UserRecord MapUserFromDomain(User u) => new()
    {

        Id = u.Id,
        Email = u.Email,
        PasswordHash = u.PasswordHash,
        CreatedAt = u.CreatedAt,
        IsAdmin = u.IsAdmin

    };

    private static User MapUserToDomain(UserRecord r)
        => new(r.Id, r.Email, r.PasswordHash, r.CreatedAt, r.IsAdmin);


    private static string BuildUsersFilePath(string filePath)
    {

        var directory = Path.GetDirectoryName(filePath);

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

        var extension = Path.GetExtension(filePath);

        var usersFileName = $"{fileNameWithoutExtension}.users{extension}";

        return string.IsNullOrWhiteSpace(directory)
            ? usersFileName
            : Path.Combine(directory, usersFileName);

    }

    private void WriteJson<T>(string path, List<T> records)
    {

        var dir = Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        var tmp = path + ".tmp";

        var json = JsonSerializer.Serialize(records, JsonOptions);

        File.WriteAllText(tmp, json);

        if (File.Exists(path))
            File.Replace(tmp, path, destinationBackupFileName: null, ignoreMetadataErrors: true);
        else
            File.Move(tmp, path);

    }


    private static string NormalizeEmail(string email)
    {

        email = (email ?? string.Empty).Trim();

        if (email.Length == 0)
            return string.Empty;

        return email.ToUpperInvariant();

    }

}
