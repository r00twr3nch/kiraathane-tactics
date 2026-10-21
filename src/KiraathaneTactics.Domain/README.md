# Domain Katmanı

**En iç çekirdek. Hiçbir şeye bağımlı değildir** (ne EF, ne HTTP, ne JWT, ne başka proje).

## Buraya ne konur?
- `Entities/` — iş nesneleri (tablolar). Örn: `Product`, `User`
- `Enums/` — sabit değer kümeleri. Örn: `UserRole`
- `Common/` — ortak taban sınıflar: `BaseEntity`, `AuditableEntity`, `ISoftDeletable`
- `Exceptions/` — domain kuralı ihlalleri. Örn: `DomainException`
- (büyüdükçe) `ValueObjects/`, `Events/`

## Buraya ne KONMAZ?
- ❌ EF Core attribute/yapılandırması (o → Infrastructure)
- ❌ DTO, validator, use-case (o → Application)
- ❌ `using Microsoft.*` türü altyapı bağımlılıkları

## Kural
İş kuralını entity'nin **içine** koy (örn. `Product.SetPrice()` negatif fiyatı reddeder).
Böylece geçersiz bir nesne hiç oluşturulamaz.

> Tam örnek için: [`docs/NEREYE-NE-KONUR.md`](../../docs/NEREYE-NE-KONUR.md)
