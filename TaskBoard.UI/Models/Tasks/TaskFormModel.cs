using System.ComponentModel.DataAnnotations;
using TaskBoard.Domain;

namespace TaskBoard.UI.Models.Tasks;

/// <summary>
/// Shared form model used by both Create and Edit pages.
/// Each page maps this to its own API model (CreateTaskModel / UpdateTaskModel).
/// </summary>
public class TaskFormModel
{

    [Required(ErrorMessage = "Titulek je povinný.")]
    [MinLength(3, ErrorMessage = "Titulek musí mít alespoň 3 znaky.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Priorita je povinná.")]
    public Priority Priority { get; set; } = Priority.Medium;

    public Status? Status { get; set; }

    public DateTime? DueDate { get; set; }

    public List<string> Tags { get; set; } = [];

}
