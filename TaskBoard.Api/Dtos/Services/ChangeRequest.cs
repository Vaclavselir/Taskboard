
using TaskBoard.Domain;

namespace TaskBoard.Api.Dtos.Services;


public sealed class UpdateTaskRequest
{

    public string? Title { get; init; }

    public string? Description { get; init; }

    public DateTime? DueDate { get; init; }

    public Status? Status { get; init; }
    
    public Priority? Priority { get; init; }

}







