using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Tiles.Common;
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Application.Features.Tiles.GetCatalog;

public sealed class GetTileCatalogQueryHandler(IGenericRepository<TileDefinition> tiles)
    : IQueryHandler<GetTileCatalogQuery, IReadOnlyList<TileDefinitionDto>>
{
    public async Task<Result<IReadOnlyList<TileDefinitionDto>>> Handle(
        GetTileCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var catalog = await tiles.Query()
            .OrderBy(t => t.Name)
            .Select(t => new TileDefinitionDto(t.Id, t.Name, t.Value, t.North, t.East, t.South, t.West))
            .ToListAsync(cancellationToken);

        return catalog;
    }
}
