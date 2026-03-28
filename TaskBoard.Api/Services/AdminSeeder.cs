using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;

namespace TaskBoard.Api.Services;

public class AdminSeeder
{

    private readonly IUserRepository _users;
    private readonly IHasher _hasher;
    private readonly IGeneratorId _ids;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(IUserRepository users, IHasher hasher, IGeneratorId ids, IConfiguration config, ILogger<AdminSeeder> logger)
    {

        _users = users;
        _hasher = hasher;
        _ids = ids;
        _config = config;
        _logger = logger;

    }

    public async Task SeedAsync(CancellationToken ct = default)
    {

        var email = _config["AdminAccount:Email"];
        var password = _config["AdminAccount:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {

            _logger.LogWarning("AdminAccount not configured — skipping admin seed.");

            return;

        }

        string normalizedEmail = User.NormalizeEmail(email);

        var existing = await _users.GetByEmailAsync(normalizedEmail, ct);

        if (existing is not null)
        {

            _logger.LogInformation("Admin user already exists, skipping seed.");

            return;

        }

        var admin = new User(
            id: _ids.NewGuid().ToString("N"),
            email: normalizedEmail,
            passwordHash: _hasher.HashPassword(password),
            createdAt: DateTime.UtcNow,
            isAdmin: true
        );

        _users.Add(admin);

        await _users.SaveAsync(ct);

        _logger.LogInformation("Admin user seeded: {Email}", normalizedEmail);
        
    }

}
