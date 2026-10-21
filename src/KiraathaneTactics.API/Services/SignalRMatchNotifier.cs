using KiraathaneTactics.API.Hubs;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Features.Matches.Common;
using Microsoft.AspNetCore.SignalR;

namespace KiraathaneTactics.API.Services;

public sealed class SignalRMatchNotifier(IHubContext<MatchHub> hub) : IMatchNotifier
{
    public Task PublishAsync(MatchStateDto state, CancellationToken cancellationToken = default) =>
        hub.Clients.Group(state.Id.ToString()).SendAsync("state", state, cancellationToken);
}
