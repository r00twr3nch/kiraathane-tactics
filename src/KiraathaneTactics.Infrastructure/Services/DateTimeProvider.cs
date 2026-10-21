using KiraathaneTactics.Application.Common.Interfaces;

namespace KiraathaneTactics.Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
