using System;
using TaskBoard.Application.Abstractions;
using TaskBoard.Domain;
using Microsoft.AspNetCore.Identity;

namespace TaskBoard.Infrastructure;

public sealed class HasherAdapter : IHasher
{

    private static readonly object NewUser = new();
    private readonly PasswordHasher<object> _hasher = new();

    public string HashPassword(string password) => _hasher.HashPassword(NewUser, password);

    public bool VerifyPassword(string password, string passwordHash)
    {

        var result = _hasher.VerifyHashedPassword(NewUser, passwordHash, password);

        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;

    }

}
