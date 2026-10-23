using KiraathaneTactics.Domain.Common;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>
/// Sistemdeki kullanıcı. Kimlik doğrulama (JWT) bu entity üzerinden yapılır.
/// </summary>
public class User : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;

    /// <summary>Kapıdan ilk yazılan ad. Duvar adı değişse de bu sabit kalır.</summary>
    public string RegisteredName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
}
