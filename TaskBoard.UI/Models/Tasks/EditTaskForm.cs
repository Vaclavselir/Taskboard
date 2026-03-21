using System.ComponentModel.DataAnnotations;
using TaskBoard.Domain;

namespace TaskBoard.UI.Models.Tasks;

public class EditTaskForm
{

    [Required(ErrorMessage = "Titulek je povinný.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Status je povinný.")]
    public Status Status { get; set; } = Status.Todo;

    [Required(ErrorMessage = "Priorita je povinná.")]
    public Priority Priority { get; set; } = Priority.Medium;

}
