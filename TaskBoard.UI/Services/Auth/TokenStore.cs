using System;

namespace TaskBoard.UI.Services.Auth;

public class TokenStore
{

    public string? Token { get; private set; }

    public void Set(string token) => Token = token;

    public void Clear() => Token = null;
    
}
