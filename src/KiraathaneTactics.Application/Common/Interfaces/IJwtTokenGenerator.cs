using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Common.Interfaces;

public sealed record JwtToken(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Verilen kullanıcı için imzalı JWT access token üretir. Implementasyonu Infrastructure'da.
/// </summary>
public interface IJwtTokenGenerator
{
    JwtToken Generate(User user);
}
