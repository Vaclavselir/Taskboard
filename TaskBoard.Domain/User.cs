using System;

namespace TaskBoard.Domain;

public sealed class User
{

    public string Id { get; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public DateTime CreatedAt { get; }

    public bool IsAdmin { get; private set; }

    public User(string email, string passwordHash, bool isAdmin = false)
        :this(
                id: Guid.NewGuid().ToString("N"),
                email: email,
                passwordHash: passwordHash,
                createdAt: DateTime.UtcNow,
                isAdmin: isAdmin
            ){}

    public User(string id, string email, string passwordHash, DateTime createdAt, bool isAdmin = false)
    {

        var normalizedEmail = NormalizeEmail(email);

        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User id cannot be empty.", nameof(id));

        if (normalizedEmail.Length > 320)
            throw new ArgumentException("Email is too long.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        Id = id.Trim();
        Email = normalizedEmail;
        PasswordHash = passwordHash.Trim();
        CreatedAt = createdAt;
        IsAdmin = isAdmin;

    }

    public void ChangeEmail(string email)
    {

        var normalizedEmail = NormalizeEmail(email);

        if (normalizedEmail.Length > 320)
            throw new ArgumentException("Email is too long.", nameof(email));

        Email = normalizedEmail;

    }

    public void ChangePasswordHash(string passwordHash)
    {

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash.Trim();

    }

    public void SetAdmin(bool isAdmin) => IsAdmin = isAdmin;

    public static string NormalizeEmail(string email)
    {

        email = (email ?? string.Empty).Trim();

        if (email.Length == 0)
            throw new ArgumentException("Email is required.", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Email must contain '@'.", nameof(email));

        return email.ToLowerInvariant();
        
    }


}
