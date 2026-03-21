using System;

namespace TaskBoard.UI.Models.Tasks;

public class PatchTaskResult
{

    public bool Success { get; set; }

    public bool Conflict { get; set; }
    
    public string? ErrorMessage { get; set; }

}
