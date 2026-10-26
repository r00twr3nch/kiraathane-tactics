using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(ApplicationDbContext context)
    : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(u => u.Email == email, cancellationToken);

    public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        => await Set.FirstOrDefaultAsync(
            u => u.RegisteredName.ToLower() == login
                || u.FullName.ToLower() == login
                || u.Email == login,
            cancellationToken);

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
        => await Set.AnyAsync(
            u => u.RegisteredName.ToLower() == name || u.FullName.ToLower() == name,
            cancellationToken);

    public Task RenameWallAsync(Guid userId, string name, CancellationToken cancellationToken = default)
        => Context.Set<WallEntry>()
            .Where(entry => entry.UserId == userId && entry.Name != name)
            .ExecuteUpdateAsync(setters => setters.SetProperty(entry => entry.Name, name), cancellationToken);
}
