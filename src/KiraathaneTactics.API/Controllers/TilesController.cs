using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Tiles.Common;
using KiraathaneTactics.Application.Features.Tiles.GetCatalog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

/// <summary>Katalog taşları. Maç kuralları burada yoktur.</summary>
[AllowAnonymous]
public sealed class TilesController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TileDefinitionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(new GetTileCatalogQuery(), cancellationToken));
}
