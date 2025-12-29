
using TaskBoard.Domain;
using TaskBoard.Application.Services;

namespace TaskBoard.Api.Dtos.Services;


public sealed record class ChangePriorityRequest
(

    string Priority

);

public sealed record class ChangeStatusRequest
(

    string Status
    
);

