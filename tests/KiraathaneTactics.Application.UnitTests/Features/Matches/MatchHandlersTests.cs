using FluentAssertions;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Features.Matches.Create;
using KiraathaneTactics.Application.Features.Matches.GetState;
using KiraathaneTactics.Application.Features.Matches.PlaceTile;
using KiraathaneTactics.Application.Features.Matches.UsePerk;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.ValueObjects;
using NSubstitute;

namespace KiraathaneTactics.Application.UnitTests.Features.Matches;

public class MatchHandlersTests
{
    private readonly IMatchRepository _matches = Substitute.For<IMatchRepository>();
    private readonly IGenericRepository<TileDefinition> _tiles = Substitute.For<IGenericRepository<TileDefinition>>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _clock = Substitute.For<IDateTimeProvider>();

    public MatchHandlersTests() => _clock.UtcNow.Returns(DateTimeOffset.UtcNow);
    private readonly IMatchNotifier _notifier = Substitute.For<IMatchNotifier>();

    [Fact]
    public async Task Create_persists_a_fresh_match()
    {
        Match? saved = null;
        _matches.AddAsync(Arg.Do<Match>(m => saved = m), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new CreateMatchCommandHandler(_matches, _unitOfWork)
            .Handle(new CreateMatchCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        saved.Should().NotBeNull();
        result.Value.Should().Be(saved!.Id);
        saved.Cells.Should().HaveCount(25);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Place_missing_match_does_not_save()
    {
        var id = Guid.NewGuid();
        _matches.GetWithBoardAsync(id, Arg.Any<CancellationToken>()).Returns((Match?)null);

        var result = await PlaceHandler().Handle(Command(id, Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Match.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Place_missing_tile_does_not_save()
    {
        var match = new Match();
        var tileId = Guid.NewGuid();
        _matches.GetWithBoardAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);
        _tiles.GetByIdAsync(tileId, Arg.Any<CancellationToken>()).Returns((TileDefinition?)null);

        var result = await PlaceHandler().Handle(Command(match.Id, tileId), CancellationToken.None);

        result.Error.Code.Should().Be("Tile.NotFound");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Place_on_the_wrong_turn_returns_the_domain_code()
    {
        var match = new Match();
        var tile = Card("Çay");
        _matches.GetWithBoardAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);
        _tiles.GetByIdAsync(tile.Id, Arg.Any<CancellationToken>()).Returns(tile);

        var result = await PlaceHandler().Handle(
            Command(match.Id, tile.Id, PlayerSide.Dark), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Match.NotYourTurn");
        match.Cells.Should().OnlyContain(c => c.IsEmpty);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Place_flips_the_weaker_neighbor_and_saves()
    {
        var match = new Match();
        var weak = Card("Zayıf", west: 1);
        match.PlaceTile(PlayerSide.Light, BoardPosition.Create(1, 1), weak);
        match.ClearDomainEvents();

        var strike = Card("Baskın", west: 6);
        _matches.GetWithBoardAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);
        _tiles.GetByIdAsync(strike.Id, Arg.Any<CancellationToken>()).Returns(strike);

        var result = await PlaceHandler().Handle(
            Command(match.Id, strike.Id, PlayerSide.Dark, row: 1, column: 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.Score(PlayerSide.Dark).Should().Be(2);
        match.Score(PlayerSide.Light).Should().Be(0);
        match.CurrentTurn.Should().Be(PlayerSide.Light);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Laf_ebeligi_spends_a_charge_and_keeps_the_turn()
    {
        var match = new Match();
        _matches.GetWithBoardAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);

        var result = await new UsePerkCommandHandler(_matches, _unitOfWork, _clock, _notifier).Handle(
            new UsePerkCommand { MatchId = match.Id, Side = PlayerSide.Light, Perk = PerkType.LafEbeligi },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        match.LightPerkCharges.Should().Be(2);
        match.CurrentTurn.Should().Be(PlayerSide.Light);
    }

    [Fact]
    public async Task Get_state_projects_scores_and_a_missing_match_is_not_found()
    {
        var missing = await new GetMatchStateQueryHandler(_matches, _clock, _unitOfWork, _notifier)
            .Handle(new GetMatchStateQuery(Guid.NewGuid()), CancellationToken.None);
        missing.IsFailure.Should().BeTrue();
        missing.Error.Code.Should().Be("Match.NotFound");

        var match = new Match();
        var tile = Card("Çay", east: 4);
        match.PlaceTile(PlayerSide.Light, BoardPosition.Create(0, 0), tile);
        _matches.GetWithBoardAsync(match.Id, Arg.Any<CancellationToken>()).Returns(match);

        var state = await new GetMatchStateQueryHandler(_matches, _clock, _unitOfWork, _notifier)
            .Handle(new GetMatchStateQuery(match.Id), CancellationToken.None);

        state.IsSuccess.Should().BeTrue();
        state.Value.LightScore.Should().Be(1);
        state.Value.Cells.Should().HaveCount(25);
        state.Value.Cells[0].Owner.Should().Be(PlayerSide.Light);
        state.Value.Cells[0].East.Should().Be(4);
        state.Value.Events.Should().ContainSingle(e => e.Kind == "TilePlaced");
    }

    private PlaceTileCommandHandler PlaceHandler() => new(_matches, _tiles, _unitOfWork, _clock, _notifier);

    private static PlaceTileCommand Command(
        Guid matchId,
        Guid tileId,
        PlayerSide side = PlayerSide.Light,
        int row = 0,
        int column = 0) => new()
    {
        MatchId = matchId,
        TileDefinitionId = tileId,
        Side = side,
        Row = row,
        Column = column
    };

    private static TileDefinition Card(string name, int north = 0, int east = 0, int south = 0, int west = 0) =>
        new(name, 1, TileEdges.Create(north, east, south, west));
}
