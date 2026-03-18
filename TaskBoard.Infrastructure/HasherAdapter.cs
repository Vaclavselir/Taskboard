using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;
using Microsoft.AspNetCore.Identity;

namespace TaskBoard.Infrastructure;

public sealed class HasherAdapter : IHasher
{

    private readonly PasswordHasher<User> _hasher = new();

    public string HashPassword(User user, string password)
        => _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password, string passwordHash)
    {

        var result = _hasher.VerifyHashedPassword(user, passwordHash, password);

        return result == PasswordVerificationResult.Success
            || result == PasswordVerificationResult.SuccessRehashNeeded;

    }

}
