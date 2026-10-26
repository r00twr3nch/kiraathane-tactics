namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// Online masa kuyruğu. Bekleyen açık koltuk sunucudadır; iki telefon aynı tarayıcıyı paylaşmaz.
/// </summary>
public interface IOnlineQueue
{
    /// <summary>
    /// Boş masa varsa koyu koltuğu ayırır. Yoksa açık koltuğu kuyruğa yazar.
    /// </summary>
    Task<QueueClaim> TakeOrOfferAsync(Func<Task<OnlineSeat>> openLight, CancellationToken cancellationToken = default);

    /// <summary>Açık koltuk hâlâ kuyrukta mı? Rakip oturduysa false döner.</summary>
    bool IsWaiting(Guid matchId);

    /// <summary>Şu an rakip bekleyen açık koltuklar.</summary>
    IReadOnlyList<OnlineSeat> Waiting { get; }

    /// <summary>Açık koltuk hâlâ bekliyorsa kuyruktan düşer.</summary>
    void Release(Guid matchId);
}

public sealed record QueueClaim(bool Waiting, OnlineSeat Seat);

public sealed record OnlineSeat(Guid MatchId, string RoomCode, string Side, string SeatToken);
