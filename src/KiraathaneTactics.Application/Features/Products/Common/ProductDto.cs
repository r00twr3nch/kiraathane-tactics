using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Products.Common;

/// <summary>Ürünün dışarı açılan okuma modeli (entity'yi doğrudan sızdırmayız).</summary>
/// <param name="Id">Ürünün benzersiz kimliği.</param>
/// <param name="Name">Ürün adı.</param>
/// <param name="Description">Ürün açıklaması (opsiyonel).</param>
/// <param name="Price">Birim fiyat.</param>
/// <param name="StockQuantity">Stok adedi.</param>
/// <param name="CreatedAt">Oluşturulma anı (UTC).</param>
public sealed record ProductDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    DateTimeOffset CreatedAt);

/// <summary>
/// Manuel mapping (AutoMapper yerine): hızlı, derleme-zamanı güvenli ve okunaklı.
/// Projeksiyon (Select) sorgularda da bu şekilde yapılır.
/// </summary>
public static class ProductMapping
{
    public static ProductDto ToDto(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.StockQuantity,
        product.CreatedAt);
}
