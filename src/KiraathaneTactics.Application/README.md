# Application Katmanı

**İş mantığının (use-case) yaşadığı yer.** Sadece Domain'e bağımlıdır. Altyapıyı (EF, JWT) **interface ile** tanır.

## Buraya ne konur?
- `Features/{Özellik}/{Aksiyon}/` — **işin çoğu burada (vertical slice)**
  - `Command` / `Query` + `Handler` + `Validator`
  - `Common/` → o özelliğin `Dto`'su, `Mapping`'i, `Errors`'u
- `Common/Messaging/` — CQRS altyapısı (`ISender`, `ICommand`, `IQuery`, handler arayüzleri)
- `Common/Behaviors/` — çapraz-kesit davranışlar (`LoggingBehavior`, `ValidationBehavior`)
- `Common/Interfaces/` — altyapı **sözleşmeleri** (`IJwtTokenGenerator`, `IUnitOfWork`, repository arayüzleri)
- `Common/Results/` — `Result`, `Error` (hata akışı)
- `Common/Models/` — ortak modeller (`ApiResponse`, `PagedResult`, `PaginatedRequest`)

## Buraya ne KONMAZ?
- ❌ Somut EF/DbContext kodu, SQL (o → Infrastructure)
- ❌ Controller, HTTP detayı (o → API)

## Kural
Bir servisin gerçek koduna ihtiyacın varsa: **arayüzünü buraya**, implementasyonunu Infrastructure'a koy.
Handler'lar ve Validator'lar `AddApplication()` ile **otomatik** kaydolur.

> Tam örnek için: [`docs/NEREYE-NE-KONUR.md`](../../docs/NEREYE-NE-KONUR.md)
