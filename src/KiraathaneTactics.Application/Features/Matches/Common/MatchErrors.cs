using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Features.Matches.Common;

public static class MatchErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Match.NotFound", $"'{id}' kimlikli maç bulunamadı.");

    public static Error TileNotFound(Guid id) =>
        Error.NotFound("Tile.NotFound", $"'{id}' kimlikli taş tanımı bulunamadı.");

        public static Error RoomNotFound(string code) =>
            Error.NotFound("Match.RoomNotFound", $"'{code}' kodlu oda bulunamadı.");
    }
