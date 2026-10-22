using FluentAssertions;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.Events;
using KiraathaneTactics.Domain.Exceptions;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Domain.UnitTests;

public class MatchRulesTests
{
    [Fact]
    public void New_match_allocates_25_empty_cells_and_starts_with_light()
    {
        var match = new Match();

        match.Cells.Should().HaveCount(25);
        match.Cells.Should().OnlyContain(c => c.IsEmpty);
        match.CurrentTurn.Should().Be(PlayerSide.Light);
        match.Status.Should().Be(MatchStatus.InProgress);
        match.LightPerkCharges.Should().Be(3);
        match.DarkPerkCharges.Should().Be(3);
    }

    [Fact]
    public void Stronger_shared_edge_flips_the_neighbor_and_does_not_chain()
    {
        var match = new Match();
        // Dark'ın batısı 2. Light doğuya 5 ile bakınca Dark'ı çevirir.
        // Çevrilen taşın kendi komşusu (daha önce konmuş ikinci bir Dark) zincire girmez.
        match.PlaceTile(PlayerSide.Light, At(2, 0), Card("Çay", 1, east: 1));
        match.PlaceTile(PlayerSide.Dark, At(2, 2), Card("Kahve", 2, west: 2));
                // Kahve komşu değil; Çay yerinde kalır. Ara taş da çevirmez.
                match.PlaceTile(PlayerSide.Light, At(0, 0), Card("Boş", 1));
                match.PlaceTile(PlayerSide.Dark, At(2, 3), Card("Yancı", 2, west: 9));

                match.PlaceTile(PlayerSide.Light, At(2, 1), Card("Nargile", 5, east: 5, west: 1));

                // Nargile Kahve'yi çevirir (doğu 5 > batı 2). Çay kendi taşı olduğu için durur.
                // Yancı, Kahve'nin komşusu olsa da zincire girmez. Skor: Çay + Boş + Nargile + Kahve.
                Owner(match, 2, 0).Should().Be(PlayerSide.Light);
                Owner(match, 2, 2).Should().Be(PlayerSide.Light);
                Owner(match, 2, 3).Should().Be(PlayerSide.Dark);
                match.Score(PlayerSide.Light).Should().Be(4);
                match.DomainEvents.OfType<TilesFlipped>().Single().Positions.Should().ContainSingle()
                    .Which.Should().Be(At(2, 2));
    }

    [Fact]
    public void Equal_edges_go_to_the_heavier_cup_and_a_missing_arrow_does_not_capture()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(1, 1), Card("Hafif", 2, east: 4));
        match.PlaceTile(PlayerSide.Dark, At(1, 2), Card("Ağır", 6, west: 4));

        Owner(match, 1, 1).Should().Be(PlayerSide.Dark);
        Cell(match, 1, 2).Value.Should().Be(6);

        match.PlaceTile(PlayerSide.Light, At(0, 2), Card("Oksuz", 9, south: 0));
        Owner(match, 1, 2).Should().Be(PlayerSide.Dark);
    }

    [Fact]
    public void Equal_edge_and_equal_weight_do_not_capture()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(1, 1), Card("Eşit", 4, east: 4));
        match.PlaceTile(PlayerSide.Dark, At(1, 2), Card("EşitKarşı", 4, west: 4));

        Owner(match, 1, 1).Should().Be(PlayerSide.Light);
    }

    [Fact]
    public void Corner_only_checks_existing_neighbors()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(0, 1), Card("Sağ", 1, west: 1));
        match.PlaceTile(PlayerSide.Dark, At(1, 0), Card("Alt", 1, north: 1));

        match.PlaceTile(PlayerSide.Light, At(0, 0), Card("Köşe", 6, east: 6, south: 6));

        Owner(match, 0, 1).Should().Be(PlayerSide.Light);
        Owner(match, 1, 0).Should().Be(PlayerSide.Light);
        match.Score(PlayerSide.Light).Should().Be(3);
    }

    [Fact]
    public void Locked_tile_cannot_be_flipped()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(2, 2), Card("Kilitli", 1, west: 1), PerkType.DemirAtma);
        match.PlaceTile(PlayerSide.Dark, At(2, 1), Card("Zorba", 9, east: 9));

        Owner(match, 2, 2).Should().Be(PlayerSide.Light);
        Cell(match, 2, 2).IsLocked.Should().BeTrue();
        match.LightPerkCharges.Should().Be(2);
    }

    [Fact]
    public void Face_down_tile_neither_captures_nor_is_captured()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(2, 2), Card("Açık", 1, west: 1));
        match.PlaceTile(PlayerSide.Dark, At(2, 1), Card("Kapalı", 9, east: 9), PerkType.GizliHamle);

        Owner(match, 2, 2).Should().Be(PlayerSide.Light);
        Cell(match, 2, 1).IsFaceDown.Should().BeTrue();

        match.PlaceTile(PlayerSide.Light, At(2, 0), Card("Baskın", 9, east: 9));
        Owner(match, 2, 1).Should().Be(PlayerSide.Dark);
        Cell(match, 2, 1).IsFaceDown.Should().BeTrue();
    }

    [Fact]
    public void Face_down_tile_opens_on_the_owners_next_placement_and_then_captures()
    {
        var match = new Match();
        match.PlaceTile(PlayerSide.Light, At(1, 1), Card("Hedef", 1, south: 1));
        match.PlaceTile(PlayerSide.Dark, At(4, 4), Card("Bekle", 1));
        match.PlaceTile(PlayerSide.Light, At(2, 1), Card("Kapalı", 9, north: 9), PerkType.GizliHamle);

        Cell(match, 2, 1).IsFaceDown.Should().BeTrue();
        Owner(match, 1, 1).Should().Be(PlayerSide.Light);
        match.LightPerkCharges.Should().Be(2);

        match.PlaceTile(PlayerSide.Dark, At(4, 3), Card("Hâlâ", 1));
        Cell(match, 2, 1).IsFaceDown.Should().BeTrue();

        match.PlaceTile(PlayerSide.Light, At(3, 3), Card("Açılış", 1));

        Cell(match, 2, 1).IsFaceDown.Should().BeFalse();
        Owner(match, 1, 1).Should().Be(PlayerSide.Light);
        match.DomainEvents.OfType<TileRevealed>().Should().ContainSingle(e => e.Position == At(2, 1));
    }

    [Fact]
    public void Wrong_turn_occupied_cell_and_finished_match_are_rejected()
    {
        var match = new Match();
        var act = () => match.PlaceTile(PlayerSide.Dark, At(0, 0), Card("Erken", 1));
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Match.NotYourTurn");

        match.PlaceTile(PlayerSide.Light, At(0, 0), Card("İlk", 1));
        var occupied = () => match.PlaceTile(PlayerSide.Dark, At(0, 0), Card("Üstüne", 2));
        occupied.Should().Throw<DomainException>().Which.Code.Should().Be("Match.CellOccupied");

        FillUntilDone(match);
        match.Status.Should().NotBe(MatchStatus.InProgress);
        var late = () => match.PlaceTile(match.CurrentTurn, At(4, 4), Card("Geç", 1));
        late.Should().Throw<DomainException>().Which.Code.Should().Be("Match.AlreadyFinished");
    }

    [Fact]
        public void Thirteen_by_flip_completes_the_match_for_light()
    {
        var match = new Match();
        for (var column = 0; column < 5; column++)
        {
            match.PlaceTile(PlayerSide.Light, At(0, column), Card($"Ust{column}", 3, south: 1));
            match.PlaceTile(PlayerSide.Dark, At(1, column), Card($"Alt{column}", 1, north: 1));
        }

        match.Score(PlayerSide.Light).Should().Be(5);
        match.Score(PlayerSide.Dark).Should().Be(5);
                match.CurrentTurn.Should().Be(PlayerSide.Light);

                for (var column = 0; column < 3; column++)
                {
                    match.PlaceTile(PlayerSide.Light, At(2, column), Card($"Vuruş{column}", 8, north: 9));
                    match.PlaceTile(PlayerSide.Dark, At(3, column), Card($"Bekle{column}", 1));
                }

                // 5 + 3 yeni + 3 çevirme = 11, Dark 5-3+3 = 5. Dördüncü vuruş 11+1+1 = 13.
                match.Status.Should().Be(MatchStatus.InProgress);
                match.Score(PlayerSide.Light).Should().Be(11);
                match.PlaceTile(PlayerSide.Light, At(2, 3), Card("Son", 8, north: 9));

        match.Score(PlayerSide.Light).Should().Be(13);
        match.Status.Should().Be(MatchStatus.Completed);
        match.Winner.Should().Be(PlayerSide.Light);
        match.Cells.Count(c => c.IsEmpty).Should().BeGreaterThan(0);
    }

    [Fact]
        public void Full_board_without_captures_gives_the_extra_tile_to_light()
    {
        var match = new Match();
            // 25 kare tek sayı. Ok yoksa çevirme olmaz: Light 13, Dark 12, tahta dolunca Light kazanır.
            // Beraberlik yalnızca bir kare kilitli/kapalı sayım dışı kalmadan skor eşitlenirse oluşur;
            // dolu tahtada 13-12 kaçınılmazdır, erken 13 kuralı da aynı skoru üretir.
            for (var i = 0; i < 24; i++)
            {
                var row = i / 5;
                var column = i % 5;
                match.PlaceTile(match.CurrentTurn, At(row, column), Card($"n{i}", 1));
            }

            match.Status.Should().Be(MatchStatus.InProgress);
            match.Score(PlayerSide.Light).Should().Be(12);
            match.Score(PlayerSide.Dark).Should().Be(12);

            match.PlaceTile(PlayerSide.Light, At(4, 4), Card("Son", 1));

            match.Cells.Should().OnlyContain(c => !c.IsEmpty);
            match.Score(PlayerSide.Light).Should().Be(13);
        match.Status.Should().Be(MatchStatus.Completed);
        match.Winner.Should().Be(PlayerSide.Light);
            }

            [Fact]
            public void Laf_ebeligi_spends_a_charge_without_placing()
    {
        var match = new Match();

        match.UsePerk(PlayerSide.Light, PerkType.LafEbeligi);

        match.LightPerkCharges.Should().Be(2);
        match.CurrentTurn.Should().Be(PlayerSide.Light);
        match.Cells.Should().OnlyContain(c => c.IsEmpty);
        match.DomainEvents.OfType<PerkSpent>().Single().Perk.Should().Be(PerkType.LafEbeligi);
    }

    [Fact]
    public void Fourth_perk_is_rejected_and_placement_perks_cannot_be_spent_alone()
    {
        var match = new Match();
        match.UsePerk(PlayerSide.Light, PerkType.LafEbeligi);
        match.PlaceTile(PlayerSide.Light, At(0, 0), Card("A", 1), PerkType.DemirAtma);
        match.PlaceTile(PlayerSide.Dark, At(0, 1), Card("B", 1));
        match.PlaceTile(PlayerSide.Light, At(0, 2), Card("C", 1), PerkType.GizliHamle);
        match.PlaceTile(PlayerSide.Dark, At(0, 3), Card("D", 1));

        var fourth = () => match.UsePerk(PlayerSide.Light, PerkType.LafEbeligi);
        fourth.Should().Throw<DomainException>().Which.Code.Should().Be("Match.NoPerkCharges");

        match.PlaceTile(PlayerSide.Light, At(1, 0), Card("E", 1));
        var alone = () => match.UsePerk(PlayerSide.Dark, PerkType.DemirAtma);
        alone.Should().Throw<DomainException>().Which.Code.Should().Be("Match.PerkRequiresPlacement");
    }

    [Fact]
    public void Out_of_range_position_and_tile_value_are_rejected()
    {
        var act = () => BoardPosition.Create(5, 0);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Match.OutOfBounds");

        var tile = () => new TileDefinition("Bozuk", 0, TileEdges.Create(1, 1, 1, 1));
        tile.Should().Throw<DomainException>().Which.Code.Should().Be("Tile.ValueOutOfRange");

        var edge = () => TileEdges.Create(10, 0, 0, 0);
        edge.Should().Throw<DomainException>().Which.Code.Should().Be("Tile.EdgeOutOfRange");
    }

    [Fact]
    public void Waiting_room_starts_when_dark_sits_and_rejects_a_second_guest()
    {
        var match = Match.OpenWaitingRoom("cay123", "light-token");

        match.Status.Should().Be(MatchStatus.Waiting);
        match.Cells.Should().HaveCount(25);
        match.RoomCode.Should().Be("CAY123");

        match.SeatDark("dark-token");
        match.Status.Should().Be(MatchStatus.InProgress);

        var again = () => match.SeatDark("other");
        again.Should().Throw<DomainException>().Which.Code.Should().Be("Match.SeatTaken");
    }

    [Fact]
    public void Opening_coin_flip_can_give_the_first_move_to_either_seat()
    {
        var lightOpens = Match.OpenWaitingRoom("tura01", "light-token", PlayerSide.Light);
        var darkOpens = Match.OpenWaitingRoom("yazi01", "light-token", PlayerSide.Dark);

        lightOpens.CurrentTurn.Should().Be(PlayerSide.Light);
        darkOpens.CurrentTurn.Should().Be(PlayerSide.Dark);

        darkOpens.SeatDark("dark-token");
        darkOpens.CurrentTurn.Should().Be(PlayerSide.Dark);
    }

    [Fact]
    public void Laf_ebeligi_cuts_fifteen_seconds_and_keeps_the_turn()
    {
        var match = new Match();
        var started = match.StartedAt!.Value;

        match.UsePerk(PlayerSide.Light, PerkType.LafEbeligi);

        match.CurrentTurn.Should().Be(PlayerSide.Light);
        match.LightPerkCharges.Should().Be(2);
        match.SecondsRemaining(PlayerSide.Light, started).Should().Be(195);
        match.SecondsRemaining(PlayerSide.Dark, started).Should().Be(210);
        match.DomainEvents.Should().ContainSingle(e => e is ClockWound);
    }

    [Fact]
    public void Clock_expiry_on_the_current_side_awards_the_other_side()
    {
        var match = new Match();
        match.UsePerk(PlayerSide.Light, PerkType.LafEbeligi);
        var later = match.StartedAt!.Value.AddSeconds(Match.MatchLengthSeconds - Match.LafEbeligiPenaltySeconds);

        match.ApplyClock(later);

        match.Status.Should().Be(MatchStatus.Completed);
        match.Winner.Should().Be(PlayerSide.Dark);
    }

    [Fact]
    public void A_side_may_place_each_tile_only_twice_and_a_flip_does_not_refund_it()
    {
        var match = new Match();
        var chess = Card("Satranç", 7, north: 7, east: 5, south: 4, west: 6);

        match.PlaceTile(PlayerSide.Light, At(0, 0), chess);
        match.PlaceTile(PlayerSide.Dark, At(4, 4), Card("Lokum", 1, north: 1));
        match.PlaceTile(PlayerSide.Light, At(0, 1), chess);
        match.PlaceTile(PlayerSide.Dark, At(4, 3), Card("Çay", 3, north: 2));

        match.CopiesLeft(PlayerSide.Light, chess.Id).Should().Be(0);
        match.CopiesLeft(PlayerSide.Dark, chess.Id).Should().Be(2);

        var act = () => match.PlaceTile(PlayerSide.Light, At(0, 2), chess);
        act.Should().Throw<DomainException>().Which.Code.Should().Be("Match.TileSpent");
    }

    private static void FillUntilDone(Match match)
    {
        for (var row = 0; row < 5 && match.Status == MatchStatus.InProgress; row++)
        {
            for (var column = 0; column < 5 && match.Status == MatchStatus.InProgress; column++)
            {
                if (Cell(match, row, column).IsEmpty)
                    match.PlaceTile(match.CurrentTurn, At(row, column), Card($"f{row}{column}", 1));
            }
        }
    }

    private static BoardPosition At(int row, int column) => BoardPosition.Create(row, column);

    private static BoardCell Cell(Match match, int row, int column) =>
        match.Cells.Single(c => c.Row == row && c.Column == column);

    private static PlayerSide? Owner(Match match, int row, int column) => Cell(match, row, column).Owner;

    private static TileDefinition Card(
        string name,
        int value,
        int north = 0,
        int east = 0,
        int south = 0,
        int west = 0) =>
        new(name, value, TileEdges.Create(north, east, south, west));
}
