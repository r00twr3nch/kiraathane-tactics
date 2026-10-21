using System.Diagnostics;
using KiraathaneTactics.Application.Common.Messaging;
using Microsoft.Extensions.Logging;

namespace KiraathaneTactics.Application.Common.Behaviors;

/// <summary>
/// Her istek için başlangıç/bitiş logu ve süre ölçümü yapar.
/// En dış halkadır; validation dahil tüm pipeline'ı sarar.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("[İstek başladı] {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            stopwatch.Stop();
            logger.LogInformation(
                "[İstek tamamlandı] {RequestName} ({ElapsedMs} ms)",
                requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(
                ex, "[İstek hata verdi] {RequestName} ({ElapsedMs} ms)",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
