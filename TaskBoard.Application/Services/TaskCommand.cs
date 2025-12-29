using TaskBoard.Domain;

namespace TaskBoard.Application.Services;


public sealed record TaskCommand
(

    string Title,

    string? Description,

    Priority Priority,

    DateTime? DueDate,

    IEnumerable<string>? Tags
    
);

