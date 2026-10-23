using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Application.Features.Matches.Common;
using KiraathaneTactics.Application.Features.Wall.GetWall;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;

namespace KiraathaneTactics.Application.Features.Wall.SignWall;

public sealed class SignWallCommandHandler(
    IMatchRepository matches,
    IGenericRepository<WallEntry> wall,
    IUserRepository users,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork,
    ISender sender)
    : ICommandHandler<SignWallCommand, WallDto>
{
    public async Task<Result<WallDto>> Handle(SignWallCommand request, CancellationToken cancellationToken)
    {
        var match = await matches.GetByIdAsync(request.MatchId, cancellationToken);
        if (match is null)
            return Result.Failure<WallDto>(MatchErrors.NotFound(request.MatchId));

        if (match.Status is not (MatchStatus.Completed or MatchStatus.Draw))
            return Result.Failure<WallDto>(Error.Conflict("Wall.MatchOpen", "Masa bitmeden duvara yazılmaz."));

        if (match.VersusBot)
            return Result.Failure<WallDto>(Error.Conflict("Wall.TeaServer", "Çaycı masası duvara yazılmaz."));

        var side = SeatOf(match, request.SeatToken);
        if (side is null)
            return Result.Failure<WallDto>(Error.Forbidden("Wall.SeatForbidden", "Bu koltuk sana ait değil."));

        var userId = currentUser.UserId;
        User? account = null;
        if (userId is not null)
            account = await users.GetByIdAsync(userId.Value, cancellationToken);

        var name = account?.FullName.Trim() ?? request.Name.Trim();
        if (name.Length is < 2 or > 24)
            return Result.Failure<WallDto>(Error.Validation("Wall.Name", "Duvar adı 2 ile 24 karakter arasında olmalı."));

        var already = userId is null
            ? await wall.AnyAsync(entry => entry.MatchId == match.Id && entry.Name == name, cancellationToken)
            : await wall.AnyAsync(entry => entry.MatchId == match.Id && entry.UserId == userId, cancellationToken);
        if (!already)
        {
            var won = match.Status == MatchStatus.Completed && match.Winner == side;
            var drew = match.Status == MatchStatus.Draw;
            await wall.AddAsync(new WallEntry
            {
                MatchId = match.Id,
                UserId = userId,
                Name = name,
                Wins = won ? 1 : 0,
                Draws = drew ? 1 : 0,
                Losses = won || drew ? 0 : 1,
                SignedAt = DateTimeOffset.UtcNow
            }, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await sender.Send(new GetWallQuery(), cancellationToken);
    }

    private static PlayerSide? SeatOf(Match match, string token)
    {
        if (string.Equals(match.LightSeatToken, token, StringComparison.Ordinal))
            return PlayerSide.Light;
        if (string.Equals(match.DarkSeatToken, token, StringComparison.Ordinal))
            return PlayerSide.Dark;
        return null;
    }
}