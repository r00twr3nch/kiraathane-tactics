using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Application.Features.Wall.GetWall;

public sealed class GetWallQueryHandler(IGenericRepository<WallEntry> wall)
    : IQueryHandler<GetWallQuery, WallDto>
{
    public async Task<Result<WallDto>> Handle(GetWallQuery request, CancellationToken cancellationToken)
    {
        var grouped = await wall.Query()
            .GroupBy(entry => new { entry.UserId, entry.Name })
            .Select(group => new
            {
                group.Key.UserId,
                group.Key.Name,
                Wins = group.Sum(entry => entry.Wins),
                Draws = group.Sum(entry => entry.Draws),
                Losses = group.Sum(entry => entry.Losses)
            })
            .ToListAsync(cancellationToken);

        var rows = grouped
            .Select(row => new WallRowDto(row.Name, row.Wins, row.Draws, row.Losses, row.Wins * 2 + row.Draws, row.UserId))
            .OrderByDescending(row => row.Points)
            .ThenByDescending(row => row.Wins)
            .ThenBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase)
            .Take(12)
            .ToList();

        return new WallDto(rows);
    }
}