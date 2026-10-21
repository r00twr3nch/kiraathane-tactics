using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Products.Create;

/// <summary>Yeni ürün oluşturma isteği. Başarılıysa oluşturulan ürünün Id'sini döner.</summary>
public sealed record CreateProductCommand : ICommand<Guid>
{
    /// <summary>Ürün adı (zorunlu, en fazla 200 karakter).</summary>
    /// <example>Kablosuz Mouse</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ürün açıklaması (opsiyonel, en fazla 2000 karakter).</summary>
    /// <example>Ergonomik, şarj edilebilir kablosuz mouse.</example>
    public string? Description { get; init; }

    /// <summary>Birim fiyat (negatif olamaz).</summary>
    /// <example>249.90</example>
    public decimal Price { get; init; }

    /// <summary>Stok adedi (negatif olamaz).</summary>
    /// <example>100</example>
    public int StockQuantity { get; init; }
}
