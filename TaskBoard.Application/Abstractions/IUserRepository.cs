using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;


public interface IUserRepository
{

    void Add(User user);

    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task SaveAsync(CancellationToken cancellationToken = default);

}
