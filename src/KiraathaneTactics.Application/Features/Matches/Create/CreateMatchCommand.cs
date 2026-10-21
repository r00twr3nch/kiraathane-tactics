using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Matches.Create;

/// <summary>Boş bir 5×5 maç açar. Light başlar, her tarafta üç joker hakkı vardır.</summary>
public sealed record CreateMatchCommand : ICommand<Guid>;
