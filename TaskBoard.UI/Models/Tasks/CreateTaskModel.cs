using System.ComponentModel.DataAnnotations;
using TaskBoard.Domain;

namespace TaskBoard.UI.Models.Tasks;

public class CreateTaskModel
{

    [Required(ErrorMessage = "Titulek je povinný.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Priorita je povinná.")]
    public Priority Priority { get; set; } = Priority.Medium;

    public DateTime? DueDate { get; set; }

    public List<string>? Tags { get; set; } = [];

}
