using System.Collections.Concurrent;
using KiraathaneTactics.Application.Common.Interfaces;

namespace KiraathaneTactics.API.Services;

/// <summary>
/// Tek süreçte yaşayan online kuyruk. İlk gelen açık koltuğu açar, sonraki gelen o koltuğa oturur.
/// </summary>
public sealed class OnlineQueue : IOnlineQueue
{
    private static readonly TimeSpan Fresh = TimeSpan.FromSeconds(90);
    private readonly ConcurrentDictionary<Guid, Offer> _offers = new();

    public async Task<QueueClaim> TakeOrOfferAsync(Func<Task<OnlineSeat>> openLight, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var pair in _offers.ToArray())
        {
            if (now - pair.Value.OpenedAt > Fresh)
            {
                _offers.TryRemove(pair.Key, out _);
                continue;
            }

            if (_offers.TryRemove(pair.Key, out var taken))
                return new QueueClaim(false, taken.Seat);
        }

        var opened = await openLight();
        _offers[opened.MatchId] = new Offer(opened, now);
        return new QueueClaim(true, opened);
    }

    public bool IsWaiting(Guid matchId) => _offers.ContainsKey(matchId);

    public void Release(Guid matchId) => _offers.TryRemove(matchId, out _);

    private sealed record Offer(OnlineSeat Seat, DateTimeOffset OpenedAt);
}
