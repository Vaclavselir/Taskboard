using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;


namespace TaskBoard.Application.Services;

public sealed class ChangeStatus
{
    

    private readonly ITaskRepository _repo;

    public event Action<Guid, Status, Status>? StatusChanged;

    public ChangeStatus(ITaskRepository repo)
    {
        
        _repo = repo;

    }

    public void ChangeSta(string ownerId, Guid id, Status newStatus)
    {
        
        if (string.IsNullOrWhiteSpace(ownerId)) throw new ArgumentException("OwnerId is required.", nameof(ownerId));
        
        if (!Enum.IsDefined<Status>(newStatus)) throw new FormatException("Invalid status");

        var Task = _repo.GetById(ownerId, id) ?? throw new KeyNotFoundException($"Task {id} not found.");

        var old = Task.Status;
        Task.UpdateStatus(newStatus);

        _repo.Save();

        StatusChanged?.Invoke(id, old, newStatus);

    }

}
