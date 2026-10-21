# API (Presentation) Katmanı

**Dış dünyaya açılan kapı.** İsteği alır, CQRS'e devreder, sonucu HTTP'ye çevirir. **İş mantığı içermez.**

## Buraya ne konur?
- `Controllers/` — endpoint'ler. `ApiControllerBase`'den türer, `Sender.Send(...)` çağırır
  - `ApiControllerBase.cs` — `Result` → `ApiResponse` zarfı dönüşümü (`HandleResult`, `HandleCreated`)
- `Middleware/` — `GlobalExceptionHandler` (tüm hatalar tek tip zarfa)
- `Services/` — web'e özel servisler. Örn: `CurrentUserService` (HttpContext'ten kullanıcı)
- `DependencyInjection.cs` — controller, Swagger, exception handler kaydı (`AddApi`)
- `Program.cs` — **composition root**: katmanları birleştirir, pipeline'ı kurar
- `appsettings*.json` — ortam yapılandırması

## Buraya ne KONMAZ?
- ❌ İş mantığı (o → Application Handler'ı). Controller'da `if/else` iş kuralı görüyorsan yanlış yerdesin.
- ❌ Doğrudan DbContext kullanımı

## Kural
Bir controller metodu ideal olarak **tek satırdır**: `Sender.Send(command)` çağırıp `HandleResult` ile döndürür.

> Tam örnek için: [`docs/NEREYE-NE-KONUR.md`](../../docs/NEREYE-NE-KONUR.md)
