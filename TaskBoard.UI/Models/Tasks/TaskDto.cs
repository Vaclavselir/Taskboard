
namespace TaskBoard.UI.Models.Tasks;

public class TaskDto
{
    
    public Guid Id { get; set; }

    public string RowVersion { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? LastCheckedAt {get; set;}

    public IReadOnlyList<string> Tags { get; set; } = [];

    public bool IsOverdue {get; set;}
    
}
