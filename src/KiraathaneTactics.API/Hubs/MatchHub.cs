using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KiraathaneTactics.API.Hubs;

/// <summary>Masa durumu yayını. Hamle burada oynanmaz; istemci yalnızca grubuna katılır.</summary>
[AllowAnonymous]
public sealed class MatchHub : Hub
{
    public Task JoinMatch(Guid matchId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
}
