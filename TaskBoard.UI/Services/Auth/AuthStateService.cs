using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace TaskBoard.UI.Services.Auth;

public class AuthStateService : AuthenticationStateProvider
{

    private readonly TokenStore _store;

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthStateService(TokenStore store)
    {

        _store = store;

    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {

        var token = _store.Token;

        if (string.IsNullOrWhiteSpace(token))
            return Task.FromResult(Anonymous);

        var claims = ParseClaimsFromJwt(token);

        var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is not null && long.TryParse(expClaim.Value, out var exp))
        {

            var expDate = DateTimeOffset.FromUnixTimeSeconds(exp);

            if (expDate <= DateTimeOffset.UtcNow)
            {

                _store.Clear();

                return Task.FromResult(Anonymous);

            }

        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));

    }

    public void MarkUserAsAuthenticated(string token)
    {

        _store.Set(token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            
    }

    public void MarkUserAsLoggedOut()
    {

        _store.Clear();

        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));

    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {

        var handler = new JwtSecurityTokenHandler();

        var token = handler.ReadJwtToken(jwt);

        return token.Claims;

    }

}
