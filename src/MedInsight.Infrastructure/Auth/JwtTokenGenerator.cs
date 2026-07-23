using System.Security.Claims;
using System.Text;
using MedInsight.Application.Abstractions.Auth;
using MedInsight.Domain.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MedInsight.Infrastructure.Auth;

public sealed class JwtTokenGenerator(IConfiguration configuration) : IJwtTokenGenerator
{
    public TokenResult Generate(User user)
    {
        var key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("'Jwt:Key' yapılandırılmamış.");
        var issuer = configuration["Jwt:Issuer"] ?? "MedInsight";
        var expiryMinutes = configuration.GetValue("Jwt:ExpiryMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = issuer,
            Expires = expiresAt,
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256),
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResult(token, expiresAt);
    }
}
