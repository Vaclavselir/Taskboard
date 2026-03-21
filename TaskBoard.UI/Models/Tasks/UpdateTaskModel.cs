using TaskBoard.Domain;

namespace TaskBoard.UI.Models.Tasks;

public class UpdateTaskModel
{

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Status? Status { get; set; }

    public Priority? Priority { get; set; }
    
}
