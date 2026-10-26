using KiraathaneTactics.Application.Common.Interfaces;

namespace KiraathaneTactics.API.Services;

/// <summary>
/// Geliştirme kuyruğu. Gerçek oyuncu yokken birkaç bot sırayla boş masaya oturur.
/// </summary>
public sealed class QueueBotHost(
    IOnlineQueue queue,
    IServiceScopeFactory scopes,
    ILogger<QueueBotHost> logger) : BackgroundService
{
    private const int Bots = 3;
    private static readonly TimeSpan[] Arrivals =
    [
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(9)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var busy = new HashSet<Guid>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                var waiting = queue.Waiting.Where(seat => busy.Add(seat.MatchId)).Take(Bots).ToList();
                for (var index = 0; index < waiting.Count; index++)
                {
                    var seat = waiting[index];
                    var delay = Arrivals[Math.Min(index, Arrivals.Length - 1)];
                    _ = SitLaterAsync(seat, delay, busy, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Kuyruk botları bu turda bakamadı.");
            }
        }
    }

    private async Task SitLaterAsync(
        OnlineSeat seat,
        TimeSpan delay,
        HashSet<Guid> busy,
        CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(delay, stoppingToken);
            if (!queue.IsWaiting(seat.MatchId))
                return;

            queue.Release(seat.MatchId);
            await using var scope = scopes.CreateAsyncScope();
            var player = scope.ServiceProvider.GetRequiredService<QueueBotPlayer>();
            logger.LogInformation("Kuyruk botu {Room} masasına oturuyor.", seat.RoomCode);
            await player.SitAndPlayAsync(seat.RoomCode, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Kuyruk botu {Room} masasını bırakmak zorunda kaldı.", seat.RoomCode);
        }
        finally
        {
            busy.Remove(seat.MatchId);
        }
    }
}
