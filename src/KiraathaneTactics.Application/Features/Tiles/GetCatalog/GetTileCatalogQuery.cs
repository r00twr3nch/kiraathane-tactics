using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Features.Tiles.Common;

namespace KiraathaneTactics.Application.Features.Tiles.GetCatalog;

public sealed record GetTileCatalogQuery : IQuery<IReadOnlyList<TileDefinitionDto>>;
