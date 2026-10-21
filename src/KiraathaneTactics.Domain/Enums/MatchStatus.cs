namespace KiraathaneTactics.Domain.Enums;

/// <summary>Maç yaşam döngüsü. Waiting, koyu taraf oda koduyla oturana kadar sürer.</summary>
public enum MatchStatus
{
    Waiting = 0,
    InProgress = 1,
    Completed = 2,
    Draw = 3
}
