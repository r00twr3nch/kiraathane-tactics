using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// User'a özel sorgular için genel repository'yi genişletir.
/// Entity'ye özel ihtiyaçlar büyüdükçe buraya eklenir.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>Bu hesabın duvar satırlarını yeni ada çeker.</summary>
    Task RenameWallAsync(Guid userId, string name, CancellationToken cancellationToken = default);
}
