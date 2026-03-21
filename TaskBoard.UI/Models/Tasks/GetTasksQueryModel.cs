using System;

namespace TaskBoard.UI.Models.Tasks;

public sealed class GetTasksQueryModel
{

    public string? Priority { get; set; }

    public string? Status { get; set; }

    public List<string> Tags { get; set; } = [];

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

}
