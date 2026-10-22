using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Application.Features.Matches.Create;
using KiraathaneTactics.Application.Features.Matches.GetState;
using KiraathaneTactics.Application.Features.Matches.JoinRoom;
using KiraathaneTactics.Application.Features.Matches.OpenRoom;
using KiraathaneTactics.Application.Features.Matches.PlaceTile;
using KiraathaneTactics.Application.Features.Matches.UsePerk;
using KiraathaneTactics.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

/// <summary>
/// Kıraathane Tactics maçları. Phase 1 yerel oturumdur; kimlik doğrulaması yoktur.
/// </summary>
[AllowAnonymous]
public sealed class MatchesController(ISender sender, IOnlineQueue queue) : ApiControllerBase(sender)
{
    /// <summary>Oda kodu üretir ve Light koltuğunu ayırır. İlk sıra yazı turadır; bot masasında Light başlar.</summary>
    [HttpPost("rooms")]
    [ProducesResponseType(typeof(ApiResponse<SeatDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> OpenRoom([FromQuery] PlayerSide? openingSide, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new OpenRoomCommand(openingSide), cancellationToken);
        return HandleCreated(result, nameof(GetState), new { id = result.IsSuccess ? result.Value.MatchId : Guid.Empty });
    }

    /// <summary>Boş online masa varsa koyu koltuğa oturtur. Yoksa açık koltuğu kuyruğa yazar.</summary>
    [HttpPost("queue")]
    [ProducesResponseType(typeof(ApiResponse<SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<SeatDto>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Queue(CancellationToken cancellationToken)
    {
        var claim = await queue.TakeOrOfferAsync(async () =>
        {
            var opened = await Sender.Send(new OpenRoomCommand(null), cancellationToken);
            if (opened.IsFailure)
                throw new InvalidOperationException(opened.Error.Description);

            var seat = opened.Value;
            return new OnlineSeat(seat.MatchId, seat.RoomCode, seat.Side.ToString(), seat.SeatToken);
        }, cancellationToken);

        if (claim.Waiting)
        {
            var light = new SeatDto(claim.Seat.MatchId, claim.Seat.RoomCode, PlayerSide.Light, claim.Seat.SeatToken);
            return Accepted(ApiResponse<SeatDto>.Ok(light, StatusCodes.Status202Accepted, "Sıradasın."));
        }

        var joined = await Sender.Send(new JoinRoomCommand(claim.Seat.RoomCode), cancellationToken);
        if (joined.IsFailure)
            return HandleResult(joined);

        return Ok(ApiResponse<SeatDto>.Ok(joined.Value));
    }

    /// <summary>Açık koltuğu online kuyruktan düşürür. Rakip çoktan oturduysa masaya dokunmaz.</summary>
    [HttpDelete("queue/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public IActionResult LeaveQueue(Guid id)
    {
        queue.Release(id);
        return Ok(ApiResponse.Ok(message: "Arama bırakıldı."));
    }

    /// <summary>Oda koduyla Dark koltuğuna oturur ve maçı başlatır.</summary>
    [HttpPost("rooms/join")]
    [ProducesResponseType(typeof(ApiResponse<SeatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> JoinRoom([FromBody] JoinRoomCommand command, CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(command, cancellationToken));

    /// <summary>Boş 5×5 tahta ile yeni bir maç açar. Light başlar.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new CreateMatchCommand(), cancellationToken);
        return HandleCreated(result, nameof(GetState), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>Tahta, skor, sıra ve bu turda biriken olayları döner.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MatchStateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetState(Guid id, CancellationToken cancellationToken)
        => HandleResult(await Sender.Send(new GetMatchStateQuery(id), cancellationToken));

    /// <summary>Sırası gelen taraf boş bir kareye katalog taşı koyar. Joker isteğe bağlıdır.</summary>
    [HttpPost("{id:guid}/tiles")]
    [ProducesResponseType(typeof(ApiResponse<MatchStateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PlaceTile(
        Guid id,
        [FromBody] PlaceTileCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.MatchId)
            return ErrorResponse(StatusCodes.Status400BadRequest,
                "Route'taki id ile gövdedeki matchId uyuşmuyor.", "IdMismatch");

        return HandleResult(await Sender.Send(command, cancellationToken));
    }

    /// <summary>Taş koymadan joker. Phase 1'de yalnızca Laf Ebeliği geçerlidir.</summary>
    [HttpPost("{id:guid}/perks")]
    [ProducesResponseType(typeof(ApiResponse<MatchStateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UsePerk(
        Guid id,
        [FromBody] UsePerkCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.MatchId)
            return ErrorResponse(StatusCodes.Status400BadRequest,
                "Route'taki id ile gövdedeki matchId uyuşmuyor.", "IdMismatch");

        return HandleResult(await Sender.Send(command, cancellationToken));
    }
}
