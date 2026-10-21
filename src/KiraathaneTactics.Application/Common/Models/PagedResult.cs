namespace KiraathaneTactics.Application.Common.Models;

/// <summary>Sayfalı liste yanıtı: veriyle birlikte sayfalama meta bilgisini taşır.</summary>
public sealed class PagedResult<T>
{
    /// <summary>Geçerli sayfadaki kayıtlar.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Geçerli sayfa numarası (1'den başlar).</summary>
    public int PageNumber { get; init; }

    /// <summary>Sayfa başına kayıt sayısı.</summary>
    public int PageSize { get; init; }

    /// <summary>Filtreye uyan toplam kayıt sayısı (tüm sayfalar).</summary>
    public int TotalCount { get; init; }

    /// <summary>Toplam sayfa sayısı.</summary>
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>Önceki sayfa var mı?</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>Sonraki sayfa var mı?</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult() { }

    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
