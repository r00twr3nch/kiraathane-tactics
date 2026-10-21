using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>Maç yazma yolu. Tahta çocukları ile birlikte yüklenir; aksi halde hamle boş tahtaya gider.</summary>
public interface IMatchRepository : IGenericRepository<Match>
{
    Task<Match?> GetWithBoardAsync(Guid id, CancellationToken cancellationToken = default);

        Task<Match?> GetByRoomCodeAsync(string roomCode, CancellationToken cancellationToken = default);
    }
