using System;

namespace TaskBoard.UI.Models.Tasks;

public sealed class TaskDetailResult
{

    public TaskDto? Task { get; set; }
    
    public string? ETag { get; set; }

}
