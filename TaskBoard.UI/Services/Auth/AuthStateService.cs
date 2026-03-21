using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using TaskBoard.UI.Models;

namespace TaskBoard.UI.Services.Auth;

public class AuthStateService : AuthenticationStateProvider
{

        private readonly TokenStore _tokenStore;

    private static readonly ClaimsPrincipal Anonymous =
        new(new ClaimsIdentity());

    public AuthStateService(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokenStore.GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(new AuthenticationState(Anonymous));

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    public Task SignInAsync(string jwtToken)
    {
        _tokenStore.SetToken(jwtToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        return Task.CompletedTask;
    }

    public Task SignOutAsync()
    {
        _tokenStore.Clear();
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(Anonymous)));

        return Task.CompletedTask;
    }

    public async Task<CurrentUserModel> GetCurrentUserAsync()
    {
        var authState = await GetAuthenticationStateAsync();
        var user = authState.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return new CurrentUserModel
            {
                IsAuthenticated = false
            };
        }

        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
            .Select(c => c.Value)
            .Distinct()
            .ToList();

        return new CurrentUserModel
        {
            IsAuthenticated = true,
            Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? user.FindFirst("sub")?.Value,
            Email = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("email")?.Value,
            IsAdmin = roles.Contains("Admin") || user.FindFirst("isAdmin")?.Value == "true",
            Roles = roles
        };
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return [];

        var payload = parts[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);

        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)
                     ?? new Dictionary<string, JsonElement>();

        var claims = new List<Claim>();

        foreach (var kvp in values)
        {
            if (kvp.Key is "role" or "roles")
            {
                if (kvp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var role in kvp.Value.EnumerateArray())
                    {
                        var roleValue = role.GetString();
                        if (!string.IsNullOrWhiteSpace(roleValue))
                            claims.Add(new Claim(ClaimTypes.Role, roleValue));
                    }
                }
                else
                {
                    var roleValue = kvp.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(roleValue))
                        claims.Add(new Claim(ClaimTypes.Role, roleValue));
                }

                continue;
            }

            var value = kvp.Value.ValueKind switch
            {
                JsonValueKind.String => kvp.Value.GetString(),
                JsonValueKind.Number => kvp.Value.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => kvp.Value.GetRawText()
            };

            if (!string.IsNullOrWhiteSpace(value))
                claims.Add(new Claim(MapClaimType(kvp.Key), value));
        }

        return claims;
    }

    private static byte[] ParseBase64WithoutPadding(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }

    private static string MapClaimType(string claimType) => claimType switch
    {
        "sub" => ClaimTypes.NameIdentifier,
        "nameid" => ClaimTypes.NameIdentifier,
        "unique_name" => ClaimTypes.Name,
        "email" => ClaimTypes.Email,
        _ => claimType
    };

}
