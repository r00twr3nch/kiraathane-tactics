using KiraathaneTactics.Application.Common.Interfaces;

namespace KiraathaneTactics.Infrastructure.Security;

/// <summary>BCrypt tabanlı parola özetleme. Salt otomatik üretilir ve hash içinde saklanır.</summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
