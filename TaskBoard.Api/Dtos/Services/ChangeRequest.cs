
using TaskBoard.Domain;
using TaskBoard.Application.Services;

namespace TaskBoard.Api.Dtos.Services;

public sealed record class UpdateTaskRequest
(

    string? Title,

    string? Description,

    DateTime? DueDate,

    string? Status,

    string? Priority

);




