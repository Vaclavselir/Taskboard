using System;
using TaskBoard.Application.Auth;

namespace TaskBoard.Application.Abstractions;

public interface IAuth
{

    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

}
