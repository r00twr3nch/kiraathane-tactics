using FluentAssertions;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.ValueObjects;
using KiraathaneTactics.Infrastructure.Persistence;
using KiraathaneTactics.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Domain.UnitTests;

public class MatchPersistenceTests
{
    [Fact]
    public async Task A_placed_tile_survives_a_reload()
    {
        var databaseName = Guid.NewGuid().ToString();
        var tile = new TileDefinition("Çay", 4, TileEdges.Create(north: 2, east: 7, south: 1, west: 0));
        Guid matchId;

        await using (var writing = NewContext(databaseName))
        {
            var matches = new MatchRepository(writing);
            var match = new Match();
            match.PlaceTile(PlayerSide.Light, BoardPosition.Create(2, 2), tile);
            match.ClearDomainEvents();
            matchId = match.Id;

            writing.TileDefinitions.Add(tile);
            await matches.AddAsync(match);
            await writing.SaveChangesAsync();
        }

        await using var reading = NewContext(databaseName);
        var reloaded = await new MatchRepository(reading).GetWithBoardAsync(matchId);

        reloaded.Should().NotBeNull();
        reloaded!.Cells.Should().HaveCount(25);
        reloaded.CurrentTurn.Should().Be(PlayerSide.Dark);
        reloaded.DomainEvents.Should().BeEmpty();

        var center = reloaded.Cells.Single(c => c.Row == 2 && c.Column == 2);
        center.Owner.Should().Be(PlayerSide.Light);
        center.TileDefinitionId.Should().Be(tile.Id);
        center.East.Should().Be(7);
        center.West.Should().Be(0);
    }

    private static ApplicationDbContext NewContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
