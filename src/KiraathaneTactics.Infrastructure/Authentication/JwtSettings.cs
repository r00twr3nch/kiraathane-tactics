namespace KiraathaneTactics.Infrastructure.Authentication;

/// <summary>appsettings.json -> "Jwt" bölümüne bağlanır (Options pattern).</summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 60;
}
