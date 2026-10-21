namespace KiraathaneTactics.Application.Features.Tiles.Common;

public sealed record TileDefinitionDto(
    Guid Id,
    string Name,
    int Value,
    int North,
    int East,
    int South,
    int West);
