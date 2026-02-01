namespace TaskBoard.Api.Helpers;

public sealed record PagedResult<T>(

    IReadOnlyList<T> Items,

    int PageNumber,

    int PageSize,

    int TotalCount
    
);