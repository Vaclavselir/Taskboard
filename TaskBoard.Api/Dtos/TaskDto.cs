using System;
using TaskBoard.Domain;
using TaskBoard.Application.Services;

namespace TaskBoard.Api.Dtos;

public sealed record TaskDto
(

    Guid Id,

    string RowVersion,

    string Title,

    string? Description,

    Status Status,

    Priority Priority,

    bool IsOverdue,

    DateTime CreatedAt,

    DateTime? UpdatedAt,

    DateTime? DueDate,

    DateTime? LastCheckedAt,
    
    IReadOnlyList<string> Tags

);


