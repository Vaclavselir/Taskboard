using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public interface IHasher
{

    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);

}
