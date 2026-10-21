using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Infrastructure.Persistence.Repositories;

public sealed class MatchRepository(ApplicationDbContext context)
    : GenericRepository<Match>(context), IMatchRepository
{
    public async Task<Match?> GetWithBoardAsync(Guid id, CancellationToken cancellationToken = default)
        => await Set
            .Include(m => m.CellsForPersistence)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

                public async Task<Match?> GetByRoomCodeAsync(string roomCode, CancellationToken cancellationToken = default)
                    => await Set
                        .Include(m => m.CellsForPersistence)
                        .FirstOrDefaultAsync(m => m.RoomCode == roomCode, cancellationToken);
            }
