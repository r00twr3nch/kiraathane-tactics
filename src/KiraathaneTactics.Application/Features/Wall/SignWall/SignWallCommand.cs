using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Wall.SignWall;

/// <summary>Biten bir insan maçından duvara isim yazar. Çaycı masası reddedilir.</summary>
public sealed record SignWallCommand(Guid MatchId, string SeatToken, string Name) : ICommand<WallDto>;
