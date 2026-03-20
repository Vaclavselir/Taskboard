namespace TaskBoard.Application.Auth;

public sealed record RegisterRequest
(

    string Email,
    string Password

);