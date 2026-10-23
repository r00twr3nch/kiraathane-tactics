using KiraathaneTactics.Domain.Common;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.Events;
using KiraathaneTactics.Domain.Exceptions;
using KiraathaneTactics.Domain.ValueObjects;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>
/// Bir maçın tamamı. 5×5 tahta, sıra, skor ve joker hakları burada yaşar.
/// Ele geçirme zincirleme değildir: yalnızca yeni konan taş komşularına bakar.
/// </summary>
public class Match : AuditableEntity
{
    public const int BoardSize = BoardPosition.Size;
    public const int CellCount = BoardSize * BoardSize;
    public const int PerksPerPlayer = 3;
    /// <summary>Bir oyuncu aynı taşı maç boyunca en fazla bu kadar kez basar.</summary>
    public const int CopiesPerTile = 2;
    public const int TilesToWinEarly = 13;
        public const int DefaultMatchLengthSeconds = 210;
        public const int MinMatchLengthSeconds = 60;
        public const int MaxMatchLengthSeconds = 600;
        public const int LafEbeligiPenaltySeconds = 15;

        private readonly List<BoardCell> _cells = [];
    private readonly List<MatchEvent> _events = [];

    /// <summary>EF Core'un 25 kareyi izlemesi için. Oyun kodu <see cref="Cells"/> kullanır.</summary>
    public List<BoardCell> CellsForPersistence => _cells;

    public MatchStatus Status { get; private set; } = MatchStatus.InProgress;
    public PlayerSide CurrentTurn { get; private set; } = PlayerSide.Light;
    public PlayerSide? Winner { get; private set; }
    public int LightPerkCharges { get; private set; } = PerksPerPlayer;
    public int DarkPerkCharges { get; private set; } = PerksPerPlayer;
        public string RoomCode { get; private set; } = string.Empty;
        public string? LightSeatToken { get; private set; }
        public string? DarkSeatToken { get; private set; }
        public DateTimeOffset? StartedAt { get; private set; }
        public DateTimeOffset? TurnDeadlineUtc { get; private set; }
        /// <summary>Sırası gelen tarafın saati en son bu anda kuruldu. Rakibin saati burada durur.</summary>
        public DateTimeOffset? TurnStartedAt { get; private set; }
        public int MatchLengthSeconds { get; private set; } = DefaultMatchLengthSeconds;
        public int LightSecondsLeft { get; private set; } = DefaultMatchLengthSeconds;
        public int DarkSecondsLeft { get; private set; } = DefaultMatchLengthSeconds;
        public int LightTimePenaltySeconds { get; private set; }
        public int DarkTimePenaltySeconds { get; private set; }
        /// <summary>Online maçta bir taraf kapıyı çektiyse true. Win perdesi bunu okur.</summary>
        public bool Walkover { get; private set; }
        /// <summary>Odayı kuran koyu rengi seçtiyse true. Gelen oyuncu o zaman açık koltuğa oturur.</summary>
        public bool HostPlaysDark { get; private set; }
        /// <summary>Koyu koltuk çaycıya aitse true. Bu masalar duvara yazılmaz.</summary>
        public bool VersusBot { get; private set; }

    public IReadOnlyCollection<BoardCell> Cells => _cells;
    public IReadOnlyCollection<MatchEvent> DomainEvents => _events;

    private Match() { }

        /// <summary>İki taraf da masada. Kural testleri ve tek ekranlı masa bunu kullanır.</summary>
        public Match(Guid? id = null)
        {
            if (id is not null)
                Id = id.Value;

            RoomCode = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
            LightSeatToken = "local-light";
            DarkSeatToken = "local-dark";
            Status = MatchStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            TurnStartedAt = StartedAt;
            TurnDeadlineUtc = StartedAt.Value.AddSeconds(45);
            AllocateBoard();
        }

        public static Match OpenWaitingRoom(string roomCode, string hostSeatToken, PlayerSide? openingSide = null, int? matchLengthSeconds = null, bool hostPlaysDark = false, bool versusBot = false)
        {
            if (string.IsNullOrWhiteSpace(roomCode))
                throw new DomainException("Oda kodu boş olamaz.", "Match.RoomCodeRequired");

            if (string.IsNullOrWhiteSpace(hostSeatToken))
                throw new DomainException("Koltuk anahtarı boş olamaz.", "Match.SeatTokenRequired");

            var length = matchLengthSeconds ?? DefaultMatchLengthSeconds;
            if (length < MinMatchLengthSeconds || length > MaxMatchLengthSeconds)
                throw new DomainException("Masa süresi 1 ile 10 dakika arasında olmalı.", "Match.LengthOutOfRange");

            var match = new Match
        {
                RoomCode = roomCode.Trim().ToUpperInvariant(),
                LightSeatToken = hostPlaysDark ? null : hostSeatToken,
                DarkSeatToken = hostPlaysDark ? hostSeatToken : null,
                Status = MatchStatus.Waiting,
                StartedAt = null,
                TurnDeadlineUtc = null,
                MatchLengthSeconds = length,
                LightSecondsLeft = length,
                DarkSecondsLeft = length,
                HostPlaysDark = hostPlaysDark,
                VersusBot = versusBot,
                CurrentTurn = openingSide ?? (Random.Shared.Next(2) == 0 ? PlayerSide.Light : PlayerSide.Dark)
            };
            match.AllocateBoard();
            return match;
        }

        private void AllocateBoard()
        {
            for (var row = 0; row < BoardSize; row++)
            {
                for (var column = 0; column < BoardSize; column++)
                    _cells.Add(new BoardCell(Id, row, column));
            }
        }

    public int Score(PlayerSide side) => _cells.Count(c => c.Owner == side);

        public int SecondsRemaining(PlayerSide side, DateTimeOffset utcNow)
        {
            var bank = side == PlayerSide.Light ? LightSecondsLeft : DarkSecondsLeft;
            var penalty = side == PlayerSide.Light ? LightTimePenaltySeconds : DarkTimePenaltySeconds;
            var running = Status == MatchStatus.InProgress
                && side == CurrentTurn
                && TurnStartedAt is not null;
            var spent = running
                ? (int)Math.Floor((utcNow - TurnStartedAt!.Value).TotalSeconds)
                : 0;
            return Math.Max(0, bank - penalty - spent);
        }

        public void SeatDark(string seatToken) => SeatGuest(seatToken);

        /// <summary>Boş koltuğa oturtur. Kuran koyu seçtiyse gelen oyuncu açık tarafa geçer.</summary>
        public void SeatGuest(string seatToken)
        {
            if (Status != MatchStatus.Waiting)
                throw new DomainException("Bu masada koltuk dolu.", "Match.SeatTaken");

            if (string.IsNullOrWhiteSpace(seatToken))
                throw new DomainException("Koltuk anahtarı boş olamaz.", "Match.SeatTokenRequired");

            if (HostPlaysDark)
                LightSeatToken = seatToken;
            else
                DarkSeatToken = seatToken;

            Status = MatchStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            TurnStartedAt = StartedAt;
            TurnDeadlineUtc = StartedAt.Value.AddSeconds(45);
        }

        /// <summary>
        /// Online masada bir taraf kapıyı çekince kalan taraf kazanır.
        /// Çaycı masasında ve tek ekranlı oyunda çağrılmaz.
        /// </summary>
        public void AwardWalkover(PlayerSide winner)
        {
            if (Status is MatchStatus.Completed or MatchStatus.Draw)
                return;

            if (Status == MatchStatus.Waiting)
                throw new DomainException("Masa daha kurulmadı.", "Match.NotStarted");

            Status = MatchStatus.Completed;
            Winner = winner;
            Walkover = true;
            TurnDeadlineUtc = null;
            _events.Add(new RivalLeft(Id, winner));
        }

        public void ApplyClock(DateTimeOffset utcNow)
        {
            if (Status != MatchStatus.InProgress)
                return;

            if (SecondsRemaining(PlayerSide.Light, utcNow) == 0 && SecondsRemaining(PlayerSide.Dark, utcNow) == 0)
            {
                FinishByScore();
                return;
            }

            if (SecondsRemaining(CurrentTurn, utcNow) == 0)
                FinishByClock(CurrentTurn == PlayerSide.Light ? PlayerSide.Dark : PlayerSide.Light);
        }

        public void PlaceTile(
        PlayerSide side,
        BoardPosition position,
        TileDefinition tile,
        PerkType? perk = null,
        DateTimeOffset? utcNow = null)
    {
        ArgumentNullException.ThrowIfNull(tile);
        EnsureCanAct(side);

        var cell = CellAt(position);
        if (!cell.IsEmpty)
            throw new DomainException("Bu kare dolu.", "Match.CellOccupied");

        if (CopiesUsed(side, tile.Id) >= CopiesPerTile)
            throw new DomainException(
                $"{tile.Name} elinde kalmadı. Her taştan {CopiesPerTile} tane var.",
                "Match.TileSpent");

        var locked = false;
        var faceDown = false;

        if (perk is not null)
        {
            SpendPerk(side, perk.Value);
            locked = perk == PerkType.DemirAtma;
            faceDown = perk == PerkType.GizliHamle;
        }

        cell.Occupy(
            side,
            tile.Id,
            tile.Value,
            tile.North,
            tile.East,
            tile.South,
            tile.West,
            locked,
            faceDown);

        _events.Add(new TilePlaced(Id, position, side, faceDown, locked));

        if (!faceDown)
            ResolveCaptures(cell, position, side);

        RevealOwnFaceDownTiles(side, position);
        AdvanceTurn(utcNow);
    }

    public void UsePerk(PlayerSide side, PerkType perk)
    {
        EnsureCanAct(side);

        if (perk != PerkType.LafEbeligi)
            throw new DomainException(
                "Yalnızca Laf Ebeliği taş koymadan kullanılır. Demir Atma ve Gizli Hamle yerleştirme ile birlikte gelir.",
                "Match.PerkRequiresPlacement");

        SpendPerk(side, perk);
                var penalty = side == PlayerSide.Light
                    ? (LightTimePenaltySeconds += LafEbeligiPenaltySeconds)
                    : (DarkTimePenaltySeconds += LafEbeligiPenaltySeconds);

                _events.Add(new ClockWound(Id, side, penalty));
            }

    public void ClearDomainEvents() => _events.Clear();

    /// <summary>Bu tarafın bu taşı kaç kez masaya koyduğu. Çevrilen taşlar sayılır, geri gelmez.</summary>
    public int CopiesUsed(PlayerSide side, Guid tileDefinitionId) =>
        _cells.Count(cell => cell.PlacedBy == side && cell.TileDefinitionId == tileDefinitionId);

    public int CopiesLeft(PlayerSide side, Guid tileDefinitionId) =>
        Math.Max(0, CopiesPerTile - CopiesUsed(side, tileDefinitionId));

    private void ResolveCaptures(BoardCell placed, BoardPosition position, PlayerSide side)
    {
        var flipped = new List<BoardPosition>();

        foreach (Direction direction in Enum.GetValues<Direction>())
        {
            var neighborPos = position.Neighbor(direction);
            if (neighborPos is null)
                continue;

            var neighbor = CellAt(neighborPos.Value);
            if (neighbor.IsEmpty || neighbor.Owner == side || neighbor.IsLocked || neighbor.IsFaceDown)
                continue;

            var attack = placed.StrengthToward(direction);
            var defense = neighbor.StrengthToward(TileEdges.Opposite(direction));
            // Kenar üstünse çevirir. Kenarlar eşitse fincanın ağırlığı (ortadaki sayı) karar verir.
            var winsEdge = attack > defense;
            var winsWeight = attack == defense && attack > 0 && placed.Value > neighbor.Value;
            if (winsEdge || winsWeight)
            {
                neighbor.FlipTo(side);
                flipped.Add(neighborPos.Value);
            }
        }

        if (flipped.Count > 0)
            _events.Add(new TilesFlipped(Id, side, flipped));
    }

    /// <summary>
    /// Gizli Hamle bir sonraki kendi hamlesine kadar kapalı kalır.
    /// Yeni konan taş hariç, sahibin kapalı taşları açılır ve o an savaşır.
    /// </summary>
    private void RevealOwnFaceDownTiles(PlayerSide side, BoardPosition justPlaced)
    {
        foreach (var cell in _cells.Where(c => c.Owner == side && c.IsFaceDown && !(c.Row == justPlaced.Row && c.Column == justPlaced.Column)))
        {
            cell.Reveal();
            var position = BoardPosition.Create(cell.Row, cell.Column);
            _events.Add(new TileRevealed(Id, position, side));
            ResolveCaptures(cell, position, side);
        }
    }

    private void AdvanceTurn(DateTimeOffset? utcNow = null)
    {
        var light = Score(PlayerSide.Light);
        var dark = Score(PlayerSide.Dark);
        var filled = _cells.All(c => !c.IsEmpty);

        if (light >= TilesToWinEarly || dark >= TilesToWinEarly || filled)
        {
                    FinishByScore();
                    return;
                }

                PassTurn(utcNow ?? DateTimeOffset.UtcNow);
            }

            private void PassTurn(DateTimeOffset utcNow)
            {
                BankRunningClock(utcNow);
                CurrentTurn = CurrentTurn == PlayerSide.Light ? PlayerSide.Dark : PlayerSide.Light;
                TurnStartedAt = utcNow;
                TurnDeadlineUtc = utcNow.AddSeconds(45);
            }

            /// <summary>Hamle biten tarafın akan saniyelerini bankasına yazar. Rakibin saati hiç dokunulmaz.</summary>
            private void BankRunningClock(DateTimeOffset utcNow)
            {
                if (TurnStartedAt is null || Status != MatchStatus.InProgress)
                    return;

                var spent = (int)Math.Floor((utcNow - TurnStartedAt.Value).TotalSeconds);
                if (spent <= 0)
                    return;

                if (CurrentTurn == PlayerSide.Light)
                    LightSecondsLeft = Math.Max(0, LightSecondsLeft - spent);
                else
                    DarkSecondsLeft = Math.Max(0, DarkSecondsLeft - spent);
            }

            private void FinishByScore()
            {
                var light = Score(PlayerSide.Light);
                var dark = Score(PlayerSide.Dark);
                if (light == dark)
                {
                    Status = MatchStatus.Draw;
                    Winner = null;
                }
                else
                {
                    Status = MatchStatus.Completed;
                    Winner = light > dark ? PlayerSide.Light : PlayerSide.Dark;
                }

                TurnDeadlineUtc = null;
            }

            private void FinishByClock(PlayerSide winner)
            {
                Status = MatchStatus.Completed;
                Winner = winner;
                TurnDeadlineUtc = null;
            }

    private void SpendPerk(PlayerSide side, PerkType perk)
    {
        var remaining = side == PlayerSide.Light ? LightPerkCharges : DarkPerkCharges;
        if (remaining <= 0)
            throw new DomainException("Joker hakkın kalmadı.", "Match.NoPerkCharges");

        remaining--;
        if (side == PlayerSide.Light)
            LightPerkCharges = remaining;
        else
            DarkPerkCharges = remaining;

        _events.Add(new PerkSpent(Id, side, perk, remaining));
    }

    private void EnsureCanAct(PlayerSide side)
    {
        if (Status != MatchStatus.InProgress)
            throw new DomainException("Maç bitmiş.", "Match.AlreadyFinished");

        if (side != CurrentTurn)
            throw new DomainException("Sıra sende değil.", "Match.NotYourTurn");
    }

    private BoardCell CellAt(BoardPosition position) =>
        _cells.Single(c => c.Row == position.Row && c.Column == position.Column);
}
