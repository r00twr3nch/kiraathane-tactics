namespace KiraathaneTactics.Application.Features.Wall;

public sealed record WallRowDto(string Name, int Wins, int Draws, int Losses, int Points, Guid? UserId = null);

public sealed record WallDto(IReadOnlyList<WallRowDto> Rows);
