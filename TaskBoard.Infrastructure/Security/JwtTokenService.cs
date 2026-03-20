using TaskBoard.Application.Abstractions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Domain;

namespace TaskBoard.Infrastructure.Security;

public sealed class JwtTokenService : IJwtToken
{

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {

        _options = options.Value;

    }

    public string CreateToken(User user)
    {

        List<Claim> claims = new List<Claim>
        {

            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("sub", user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")

        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var token = new JwtSecurityToken(

            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials

        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

}
