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

    DateTime CreatedAt,

    DateTime? DueDate,
    
    IReadOnlyList<string> Tags

);


