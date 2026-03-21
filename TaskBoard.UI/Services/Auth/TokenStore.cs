using System;

namespace TaskBoard.UI.Services.Auth;

public class TokenStore
{

    private string? _token;

    public void SetToken(string token) => _token = token;
    public string? GetToken() => _token;
    public void Clear() => _token = null;

}
