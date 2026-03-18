using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public interface IHasher
{

    string HashPassword(User user, string password);

    bool VerifyPassword(User user, string password, string passwordHash);

}
