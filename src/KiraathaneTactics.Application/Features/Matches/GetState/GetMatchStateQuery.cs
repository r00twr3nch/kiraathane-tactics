using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Matches.Common;

namespace KiraathaneTactics.Application.Features.Matches.GetState;

public sealed record GetMatchStateQuery(Guid MatchId) : IQuery<MatchStateDto>;
