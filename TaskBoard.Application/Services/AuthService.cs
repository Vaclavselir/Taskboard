using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Application.Auth;
using TaskBoard.Domain;


namespace TaskBoard.Application.Services;

public sealed class AuthService : IAuth
{

    private readonly IUserRepository _userRepository;
    private readonly IHasher _passwordHasher;
    private readonly IJwtToken _jwtTokenService;
    private readonly IGeneratorId _generatorId;

    public AuthService(IUserRepository userRepository, IHasher passwordHasher, IJwtToken jwtTokenService, IGeneratorId generatorId)
    {

        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _generatorId = generatorId;

    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {

        string email = User.NormalizeEmail(request.Email);

        bool exists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);

        if (exists)
            throw new InvalidOperationException("User with this email already exists.");

        string passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User(
            id: _generatorId.NewGuid().ToString("N"),
            email: email,
            passwordHash: passwordHash,
            createdAt: DateTime.UtcNow,
            isAdmin: false
        );

        _userRepository.Add(user);

        await _userRepository.SaveAsync(cancellationToken);

        string token = _jwtTokenService.CreateToken(user);

        return new AuthResponse(token);

    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {

        string email = User.NormalizeEmail(request.Email);

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password.");

        bool isValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isValid)
            throw new UnauthorizedAccessException("Invalid email or password.");

        string token = _jwtTokenService.CreateToken(user);

        return new AuthResponse(token);

    }

}
