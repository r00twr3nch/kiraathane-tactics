namespace KiraathaneTactics.Application.Common.Interfaces;

/// <summary>
/// Zamanı soyutlar. DateTimeOffset.UtcNow yerine bunu kullanmak test edilebilirliği artırır.
/// </summary>
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}
