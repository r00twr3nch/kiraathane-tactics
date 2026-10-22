using System.Collections.Concurrent;
using KiraathaneTactics.Application.Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using KiraathaneTactics.Application.Features.Matches.LeaveSeat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KiraathaneTactics.API.Hubs;

/// <summary>
/// Masa durumu yayını. Hamle burada oynanmaz.
/// Online koltuk bağlantısı kopunca kalan tarafa hükmen galibiyet yazılır.
/// </summary>
[AllowAnonymous]
public sealed class MatchHub(IServiceScopeFactory scopes) : Hub
{
    private static readonly ConcurrentDictionary<string, SeatLink> Links = new();

    public Task JoinMatch(Guid matchId)
    {
        Links.TryRemove(Context.ConnectionId, out _);
        return Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
    }

    /// <summary>Yalnızca iki kişilik online masa bu kaydı tutar. Çaycı masası kaydolmaz.</summary>
    public Task Sit(Guid matchId, string seatToken)
    {
        if (!string.IsNullOrWhiteSpace(seatToken))
            Links[Context.ConnectionId] = new SeatLink(matchId, seatToken);

        return Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Links.TryRemove(Context.ConnectionId, out var link))
        {
            // withAutomaticReconnect kısa kopuşta aynı koltuğu geri oturtur.
            // Mühlet dolmadan dönmezse kapıyı çekmiş sayılır.
            _ = ForfeitIfStillGoneAsync(link);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Test ve açık çıkış. Kopuşla aynı hükmen galibiyeti, mühletsiz yazar.</summary>
    public Task Leave(Guid matchId, string seatToken)
    {
        Links.TryRemove(Context.ConnectionId, out _);
        return ForfeitNowAsync(new SeatLink(matchId, seatToken));
    }

    private async Task ForfeitIfStillGoneAsync(SeatLink link)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(8));
            var returned = Links.Values.Any(other =>
                other.MatchId == link.MatchId && other.SeatToken == link.SeatToken);
            if (returned)
                return;

            await ForfeitNowAsync(link);
        }
        catch
        {
            // Kopan bağlantı maçı kilitlemesin. Kalan taraf bir sonraki yoklamada da görür.
        }
    }

    private async Task ForfeitNowAsync(SeatLink link)
    {
        await using var scope = scopes.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new LeaveSeatCommand(link.MatchId, link.SeatToken), CancellationToken.None);
    }

    private sealed record SeatLink(Guid MatchId, string SeatToken);
}
