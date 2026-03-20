using System;
using TaskBoard.Domain;

namespace TaskBoard.Application.Abstractions;

public interface IJwtToken
{

    string CreateToken (User user);

}
