using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace KiraathaneTactics.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(
    IOptions<JwtSettings> jwtOptions,
    IDateTimeProvider dateTimeProvider)
    : IJwtTokenGenerator
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public JwtToken Generate(User user)
    {
        var expiresAt = dateTimeProvider.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtToken(accessToken, expiresAt);
    }
}
