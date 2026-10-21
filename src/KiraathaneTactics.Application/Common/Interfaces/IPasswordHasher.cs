namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>Parola özetleme/doğrulama. Implementasyonu Infrastructure'da (BCrypt).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
