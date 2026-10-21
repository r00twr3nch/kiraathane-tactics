namespace KiraathaneTactics.Application.Common.Models;

/// <summary>
/// Tüm sayfalı listeleme istekleri için ortak taban DTO.
/// Sayfalama alanları her query'de tekrar yazılmasın diye buradadır;
/// listeleme query'leri bundan türeyip yalnızca kendi filtre alanlarını ekler.
/// </summary>
/// <example>
/// public sealed record GetProductsQuery : PaginatedRequest, IQuery&lt;PagedResult&lt;ProductDto&gt;&gt;
/// {
///     public string? Search { get; init; }
/// }
/// </example>
public abstract record PaginatedRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    /// <summary>Kaçıncı sayfa (1'den başlar). 1'den küçük değerler 1'e çekilir.</summary>
    /// <example>1</example>
    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Sayfa başına kayıt sayısı. 1-100 aralığında sınırlanır (varsayılan 10).</summary>
    /// <example>10</example>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value switch
        {
            < 1 => DefaultPageSize,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }
}
