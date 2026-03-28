using TaskBoard.Domain;

namespace TaskBoard.Api.Dtos;

public sealed record AdminTaskDto
(

    Guid Id,
    string OwnerId,
    string Title,
    string? Description,
    Status Status,
    Priority Priority,
    bool IsOverdue,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DueDate,
    IReadOnlyList<string> Tags
    
);