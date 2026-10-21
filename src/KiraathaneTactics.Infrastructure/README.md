# Infrastructure Katmanı

**Application'daki soyutlamaların (interface) somut hali.** "Nasıl" sorularının cevabı burada.

## Buraya ne konur?
- `Persistence/`
  - `ApplicationDbContext.cs` — EF Core bağlamı (`IUnitOfWork`'i de uygular)
  - `Configurations/` — EF tablo ayarları (kolon, index, ilişki). Örn: `ProductConfiguration`
  - `Repositories/` — repository implementasyonları
  - `Interceptors/` — `AuditableEntityInterceptor` (otomatik audit + soft delete)
  - `Migrations/` — EF migration'ları
- `Authentication/` — `JwtTokenGenerator`, `JwtSettings`
- `Security/` — `PasswordHasher` (BCrypt)
- `Services/` — diğer dış/teknik servisler. Örn: `DateTimeProvider`
- `DependencyInjection.cs` — buradaki tüm servisleri DI'a kaydeder (`AddInfrastructure`)

## Buraya ne KONMAZ?
- ❌ İş kuralı / use-case (o → Application Handler'ı)
- ❌ Controller (o → API)

## Kural
Application'da `IEmailSender` arayüzü açtıysan, gerçek `EmailSender`'ı **buraya** koy ve
`DependencyInjection.cs`'te `services.AddScoped<IEmailSender, EmailSender>();` ile kaydet.

> Tam örnek için: [`docs/NEREYE-NE-KONUR.md`](../../docs/NEREYE-NE-KONUR.md)
