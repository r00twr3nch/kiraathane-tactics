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
}
