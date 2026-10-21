using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Products.Update;

/// <summary>Var olan ürünü güncelleme isteği. Id, route ile aynı olmalıdır.</summary>
public sealed record UpdateProductCommand : ICommand
{
    /// <summary>Güncellenecek ürünün kimliği (route'taki id ile aynı olmalı).</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>Ürün adı (zorunlu, en fazla 200 karakter).</summary>
    /// <example>Kablosuz Mouse (v2)</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Ürün açıklaması (opsiyonel, en fazla 2000 karakter).</summary>
    /// <example>Geliştirilmiş sensörlü yeni model.</example>
    public string? Description { get; init; }

    /// <summary>Birim fiyat (negatif olamaz).</summary>
    /// <example>279.90</example>
    public decimal Price { get; init; }

    /// <summary>Stok adedi (negatif olamaz).</summary>
    /// <example>50</example>
    public int StockQuantity { get; init; }
}
