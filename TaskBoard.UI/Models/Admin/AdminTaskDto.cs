using TaskBoard.Domain;
namespace TaskBoard.UI.Models.Admin;

public record class AdminTaskDto(
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