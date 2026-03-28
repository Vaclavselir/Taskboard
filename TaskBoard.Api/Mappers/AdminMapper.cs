using System;
using TaskBoard.Api.Dtos;
using TaskBoard.Domain;

namespace TaskBoard.Api.Mappers;

public static class AdminMapper
{

    public static AdminTaskDto ToAdminDto(this TaskItem t) =>
    new(
        t.Id,
        t.OwnerId,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.IsOverdue,
        t.CreatedAt,
        t.UpdatedAt,
        t.DueDate,
        t.Tags.Select(x => x.ToString()).ToList()
    );

}
