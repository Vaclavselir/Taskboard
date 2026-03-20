using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Auth;


namespace TaskBoard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{

    private readonly IAuth _authService;

    public AuthController(IAuth authService)
    {

        _authService = authService;

    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {

        var response = await _authService.RegisterAsync(request, cancellationToken);

        return Ok(response);

    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request,CancellationToken cancellationToken)
    {

        var response = await _authService.LoginAsync(request, cancellationToken);

        return Ok(response);

    }

}
