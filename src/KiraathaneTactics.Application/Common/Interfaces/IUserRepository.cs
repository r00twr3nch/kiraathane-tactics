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
}
