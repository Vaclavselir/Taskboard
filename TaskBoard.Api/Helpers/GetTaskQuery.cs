using System;
using TaskBoard.Domain;

namespace TaskBoard.Api.Helpers;

public sealed class GetTaskQuery
{

   public Priority? Priority { get; init; }

    public Status? Status { get; init; }

    public List<string>? Tags { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

}
