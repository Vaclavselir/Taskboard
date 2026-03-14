using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;


public interface IUserRepository
{

    void Add(User user);

    User? GetById(string id);

    User? GetByEmail(string email);

    bool ExistsByEmail(string email);

    void Save();

}
