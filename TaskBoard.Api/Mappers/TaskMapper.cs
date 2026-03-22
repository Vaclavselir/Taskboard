using System;
using TaskBoard.Api.Dtos;
using TaskBoard.Domain;

namespace TaskBoard.Api.Mappers;

public static class TaskMapper
{

    public static TaskDto ToDto(this TaskItem t) =>
    new(
    
        t.Id,

        Convert.ToBase64String(t.RowVersion),

        t.Title,

        t.Description,

        t.Status,

        t.Priority,

        t.IsOverdue,

        t.CreatedAt,

        t.DueDate,

        t.Tags.Select(x => x.ToString()).ToList()

    );


}
