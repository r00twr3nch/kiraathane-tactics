using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>Tahta değişince aynı masadaki tarayıcılara durumu iter. Kurallar burada yoktur.</summary>
public interface IMatchNotifier
{
    Task PublishAsync(MatchStateDto state, CancellationToken cancellationToken = default);
}
