using KiraathaneTactics.Domain.Common;
using KiraathaneTactics.Domain.Exceptions;

namespace KiraathaneTactics.Domain.Entities;

/// <summary>
/// Örnek iş entity'si. CRUD + CQRS akışını uçtan uca göstermek için kullanılır.
/// Yeni projende bu entity'yi kendi domain'ine göre değiştir/sil.
/// </summary>
public class Product : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    // EF Core için parametresiz ctor
    private Product() { }

    public Product(string name, string? description, decimal price, int stockQuantity)
    {
        SetName(name);
        SetPrice(price);
        Description = description;
        StockQuantity = stockQuantity;
    }

    public void Update(string name, string? description, decimal price, int stockQuantity)
    {
        SetName(name);
        SetPrice(price);
        Description = description;
        StockQuantity = stockQuantity;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Ürün adı boş olamaz.", "Product.NameRequired");
        Name = name.Trim();
    }

    private void SetPrice(decimal price)
    {
        if (price < 0)
            throw new DomainException("Fiyat negatif olamaz.", "Product.PriceNegative");
        Price = price;
    }
}
