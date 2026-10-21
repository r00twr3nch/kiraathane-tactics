# Kıraathane Tactics — .NET 10 Clean Architecture + CQRS

5×5, sıra tabanlı, yerel kıraathane temalı PvP taş oyunu. CleanTemplate iskeletinden türetildi.
İş kuralları Domain'de (`Match.PlaceTile`), use-case'ler `Features/Matches` dilimlerinde yaşar.

> **Stack:** .NET 10 · ASP.NET Core Web API (Controller) · EF Core 10 (SQL Server) · JWT · FluentValidation · Serilog · Swagger

> 🧭 **Yeni başlıyorsan buradan başla:** [`docs/NEREYE-NE-KONUR.md`](docs/NEREYE-NE-KONUR.md) —
> "neyi nereye koyarım?" hızlı haritası + sıfırdan adım adım çalışan örnek (`Category` özelliği).
> Ayrıca her katmanın kendi `README.md`'si vardır (klasöre girince ne koyacağını söyler).

---

## 🧱 Mimari (Clean Architecture)

Bağımlılıklar **daima içe doğru** akar. İç katmanlar dışı bilmez.

```
┌─────────────────────────────────────────────────────────┐
│                        API (Presentation)                │  Controller, Middleware, DI kökü
│   ┌─────────────────────────────────────────────────┐   │
│   │                  Infrastructure                  │   │  EF Core, JWT, Repository, dış servisler
│   │   ┌─────────────────────────────────────────┐   │   │
│   │   │              Application                 │   │   │  CQRS, Behavior, Validator, soyut arayüzler
│   │   │   ┌─────────────────────────────────┐   │   │   │
│   │   │   │            Domain               │   │   │   │  Entity, Value Object, Domain kuralları
│   │   │   └─────────────────────────────────┘   │   │   │
│   │   └─────────────────────────────────────────┘   │   │
│   └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘

Referans yönü:  API → Infrastructure → Application → Domain
```

| Katman | Sorumluluk | Neyi bilmez |
|---|---|---|
| **Domain** | Entity, enum, domain kuralları/exception | Hiçbir şeyi (bağımsız) |
| **Application** | Use-case'ler: CQRS Command/Query + Handler, validation, soyut servis arayüzleri | EF, SQL, HTTP, JWT |
| **Infrastructure** | Soyutlamaların somut hali: DbContext, repository, JWT üretimi, BCrypt | Controller'ları |
| **API** | HTTP uçları, pipeline, composition root | İş mantığını (CQRS'e devreder) |

---

## 📁 Klasör Yapısı

```
KiraathaneTactics/
├─ src/
│  ├─ KiraathaneTactics.Domain/
│  │  ├─ Common/         BaseEntity, AuditableEntity, ISoftDeletable, IAuditableEntity
│  │  ├─ Entities/       User, Product (örnek)
│  │  ├─ Enums/          UserRole
│  │  └─ Exceptions/     DomainException
│  │
│  ├─ KiraathaneTactics.Application/
│  │  ├─ Common/
│  │  │  ├─ Messaging/   ISender, IRequest, ICommand, IQuery, Handler'lar, Sender, IPipelineBehavior
│  │  │  ├─ Behaviors/   LoggingBehavior, ValidationBehavior
│  │  │  ├─ Results/     Result, Result<T>, Error, ErrorType
│  │  │  ├─ Interfaces/  IGenericRepository, IUserRepository, IUnitOfWork,
│  │  │  │               IJwtTokenGenerator, IPasswordHasher, ICurrentUserService, IDateTimeProvider
│  │  │  ├─ Models/      PagedResult, PaginationParams
│  │  │  └─ Extensions/  QueryableExtensions (ToPagedResultAsync)
│  │  ├─ Features/       ← İŞİN ÇOĞU BURADA (dikey dilimler / vertical slices)
│  │  │  ├─ Authentication/  Register, Login
│  │  │  └─ Products/        Create, Update, Delete, GetById, GetList
│  │  └─ DependencyInjection.cs   → AddApplication()
│  │
│  ├─ KiraathaneTactics.Infrastructure/
│  │  ├─ Persistence/
│  │  │  ├─ Configurations/  EF Core entity yapılandırmaları
│  │  │  ├─ Interceptors/    AuditableEntityInterceptor (audit + soft delete)
│  │  │  ├─ Repositories/    GenericRepository, UserRepository
│  │  │  ├─ Migrations/      EF Core migration'ları
│  │  │  ├─ ApplicationDbContext.cs
│  │  │  ├─ ApplicationDbContextFactory.cs       (design-time)
│  │  │  └─ ApplicationDbContextInitializer.cs   (migrate + seed)
│  │  ├─ Authentication/  JwtSettings, JwtTokenGenerator
│  │  ├─ Security/        PasswordHasher (BCrypt)
│  │  ├─ Services/        DateTimeProvider
│  │  └─ DependencyInjection.cs   → AddInfrastructure(configuration)
│  │
│  └─ KiraathaneTactics.API/
│     ├─ Controllers/     ApiControllerBase, AuthController, ProductsController
│     ├─ Middleware/      GlobalExceptionHandler (IExceptionHandler)
│     ├─ Services/        CurrentUserService
│     ├─ DependencyInjection.cs   → AddApi()
│     ├─ Program.cs       (composition root)
│     └─ appsettings.json
│
├─ tests/
│  └─ KiraathaneTactics.Application.UnitTests/   (xUnit + NSubstitute + FluentAssertions)
│
├─ Directory.Build.props      Tüm projeler için ortak ayarlar (net10, nullable, analyzer)
├─ Directory.Packages.props   Merkezi paket sürüm yönetimi (CPM) — TEK kaynak
├─ global.json                SDK sürümü sabitleme
├─ .editorconfig              Kod stili kuralları
└─ KiraathaneTactics.slnx         Solution
```

---

## 🚀 Çalıştırma

**Önkoşullar:** .NET 10 SDK · SQL Server veya LocalDB

```bash
# 1) Bağımlılıkları yükle
dotnet restore

# 2) (İlk kez) Geliştirme ortamında çalıştır — açılışta DB otomatik migrate edilir + admin seed edilir
dotnet run --project src/KiraathaneTactics.API --launch-profile https

# Tarayıcı otomatik açılır → https://localhost:7060/swagger
# (kök adres / de otomatik olarak /swagger'a yönlenir)
```

**Seed admin:** `admin@kiraathane.local` / `Admin123!`

> Bağlantı dizesini `src/KiraathaneTactics.API/appsettings.Development.json` içinden ayarla.
> Varsayılan: `(localdb)\MSSQLLocalDB`.

---

## 📦 API Yanıt Sözleşmesi (tek tip zarf)

**Her yanıt** — başarı da hata da — aynı `ApiResponse` zarfında döner. Böylece web/iOS/Android
istemciler tek bir generic model + tek interceptor ile tüm yanıtları işleyebilir.
HTTP durum kodu da doğru gelir (200/201/400/401/403/404/409), yani istemci hem koddan hem gövdeden okuyabilir.

```jsonc
// Başarı (200) — örn. login
{ "success": true,  "statusCode": 200, "message": "İşlem başarılı", "data": { /* ... */ }, "errors": null }

// Doğrulama hatası (400) — alan bazlı
{ "success": false, "statusCode": 400, "message": "Bir veya daha fazla doğrulama hatası oluştu.",
  "data": null, "errors": [ { "code": "Validation", "field": "Email", "message": "Geçerli bir e-posta giriniz." } ] }

// İş/yetki hatası (401/403/404/409)
{ "success": false, "statusCode": 401, "message": "E-posta veya parola hatalı.",
  "data": null, "errors": [ { "code": "Auth.InvalidCredentials", "field": null, "message": "E-posta veya parola hatalı." } ] }
```

| Alan | Açıklama |
|---|---|
| `success` | İşlem başarılı mı (istemci ilk buna bakar) |
| `statusCode` | HTTP durum kodu (gövdede de tekrarlanır) |
| `message` | Kullanıcıya gösterilebilir mesaj |
| `data` | Başarıda asıl veri; hata/boş durumda `null` |
| `errors` | Hata listesi: `code` (programatik), `field` (doğrulamada alan), `message` (gösterim) |

Tüm hatalar — handler'dan dönen `Result`, fırlatılan exception'lar ve **JWT 401/403** dahil —
bu zarfa çevrilir (`ApiResponseGlobalExceptionHandler` + `JwtBearerEvents`).
Controller'lar `HandleResult(...)` / `HandleCreated(...)` yardımcılarıyla bu zarfı otomatik üretir.

> **Mobil tarafı için ipucu:** `success` false ise `errors` listesini göster; `field` doluysa ilgili form
> alanının altına yaz. `data` modelini doğrudan kendi DTO'na bağla.

### Hata Kodları (her hatanın kendine has, kararlı kodu)

Her hata senaryosu **benzersiz** bir `code` taşır. İstemci bu kod üzerinden programatik karar verir
(özel ekran, i18n çeviri, form alanı vurgulama). Konvansiyon: `{Alan}.{Konu}.{Durum}`.

| Kod | HTTP | Anlamı |
|---|---|---|
| `Auth.Email.Required` / `Auth.Email.Invalid` / `Auth.Email.TooLong` | 400 | E-posta doğrulama |
| `Auth.Password.Required` / `.TooShort` / `.MissingUppercase` / `.MissingLowercase` / `.MissingDigit` | 400 | Parola kuralları |
| `Auth.FullName.Required` / `.TooLong` | 400 | Ad soyad doğrulama |
| `Auth.EmailAlreadyExists` | 409 | E-posta zaten kayıtlı |
| `Auth.InvalidCredentials` | 401 | E-posta/parola hatalı |
| `Auth.UserInactive` | 403 | Hesap pasif |
| `Auth.Unauthorized` | 401 | Token yok/geçersiz/süresi dolmuş |
| `Auth.Forbidden` | 403 | Rol/izin yetersiz |
| `Product.Name.Required` / `.TooLong` | 400 | Ürün adı doğrulama |
| `Product.Price.Negative` / `Product.Stock.Negative` | 400 | Ürün fiyat/stok doğrulama |
| `Product.NotFound` | 404 | Ürün bulunamadı |
| `Product.NameRequired` / `Product.PriceNegative` | 400 | Domain kuralı (savunma katmanı) |
| `Server.Unexpected` | 500 | Beklenmeyen sunucu hatası |

**Yeni kod nasıl eklenir?**
- **İş hatası (Result):** ilgili feature'ın `*Errors.cs` dosyasına `Error.Conflict("Area.Name", "mesaj")` ekle.
- **Doğrulama:** validator'da `.WithErrorCode("Area.Field.Rule")` kullan (kod vermezsen `Validation.{Field}`'e düşer).
- **Domain kuralı:** `throw new DomainException("mesaj", "Area.Name")`.

---

## 🌍 Ortamlar (Development / Test / Canlı)

Davranış `ASPNETCORE_ENVIRONMENT` değişkenine göre değişir. Her ortam, `appsettings.json` (taban)
üzerine kendi `appsettings.{Ortam}.json` dosyasını **bindirir**.

| Ortam | Dosya | Swagger | Otomatik migrate + seed | Log seviyesi |
|---|---|:---:|:---:|---|
| **Development** | `appsettings.Development.json` | ✅ | ✅ | Debug |
| **Staging (test)** | `appsettings.Staging.json` | ✅ | ❌ (CI/CD ile) | Information |
| **Production (canlı)** | `appsettings.Production.json` | ❌ | ❌ (CI/CD ile) | Warning |

- **Swagger** yalnızca canlı (Production) ortamda kapalıdır; geliştirme ve test ortamında açıktır.
- **Otomatik migrate/seed** yalnızca Development'tadır. Test/canlıda migration'lar
  `dotnet ef database update` ile (tercihen deploy pipeline'ında) uygulanır.
- **Gizli bilgiler dosyada tutulmaz.** Test/canlı için bağlantı dizesi ve JWT secret
  **ortam değişkeninden** gelir ve taban dosyadaki değerleri ezer:
  ```bash
  ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..."
  Jwt__Secret="uzun-rastgele-gizli-anahtar"
  ```
- Ortamı seçmek için: `ASPNETCORE_ENVIRONMENT=Production` (veya `Staging`).
  Lokal denemek istersen `Production-local` launch profilini kullanabilirsin.

---

## 🗄️ Veritabanı / Migration Komutları

`dotnet-ef` aracı kurulu değilse: `dotnet tool install --global dotnet-ef --version 10.0.5`

```bash
# Yeni migration ekle
dotnet ef migrations add <MigrationAdi> \
  --project src/KiraathaneTactics.Infrastructure \
  --startup-project src/KiraathaneTactics.API \
  --output-dir Persistence/Migrations

# Veritabanına uygula
dotnet ef database update \
  --project src/KiraathaneTactics.Infrastructure \
  --startup-project src/KiraathaneTactics.API
```

> Geliştirme ortamında migration'lar uygulama açılışında otomatik uygulanır
> (`Program.cs` → `InitializeDatabaseAsync`). Production'da bu satırı kaldırıp
> migration'ları CI/CD ile uygulaman önerilir.

---

## ➕ Yeni Bir Özellik (Feature) Ekleme

Her özellik **dikey dilim**: kendi Command/Query + Handler + Validator dosyalarına sahiptir.
Örnek olarak `Order` ekleyelim:

1. **Domain:** `Domain/Entities/Order.cs` (gerekirse `AuditableEntity`'den türet).
2. **Infrastructure:** `Persistence/Configurations/OrderConfiguration.cs` + `ApplicationDbContext`'e `DbSet<Order> Orders`.
3. **Application:** `Features/Orders/Create/` altına:
   - `CreateOrderCommand.cs`     → `: ICommand<Guid>`
   - `CreateOrderCommandValidator.cs`
   - `CreateOrderCommandHandler.cs` → `: ICommandHandler<CreateOrderCommand, Guid>`
4. **API:** `Controllers/OrdersController.cs` → `ApiControllerBase`'den türet, `Sender.Send(...)` çağır.
5. **Migration** ekle ve uygula.

Handler, Validator ve repository **otomatik** DI'a kaydolur (assembly tarama). Ekstra kayıt gerekmez.

> **Sayfalama:** Listeleme query'lerini `PaginatedRequest`'ten türet — `PageNumber` ve `PageSize`
> (1-100 arası, otomatik sınırlı) hazır gelir; sen yalnızca kendi filtre alanlarını eklersin.
> Örn: `public sealed record GetXQuery : PaginatedRequest, IQuery<PagedResult<XDto>> { public string? Search { get; init; } }`

> **Swagger dokümantasyonu:** Endpoint ve alan açıklamaları XML yorumlarından (`<summary>`, `<remarks>`,
> `<param>`, `<response>`, `<example>`) otomatik üretilir. Yeni endpoint/DTO yazarken bu yorumları ekle;
> Swagger'da açıklamalar ve örnek değerler kendiliğinden görünür. (XML üretimi `Directory.Build.props`'ta açık.)

### Akış (request → response)

```
HTTP → Controller → Sender.Send(command)
     → [LoggingBehavior] → [ValidationBehavior] → Handler
     → Repository / UnitOfWork → DbContext (audit + soft delete interceptor)
     → Result<T> → Controller → HTTP (200/201/204 veya 400/404/409 ...)
```

---

## 🔁 Projeyi Kendi Adınla Kullanma

Tek yapman gereken `KiraathaneTactics` kelimesini kendi proje adınla değiştirmek:

**Windows (PowerShell):**
```powershell
$old = "KiraathaneTactics"; $new = "SeninProjen"
# Dosya içerikleri
Get-ChildItem -Recurse -File -Include *.cs,*.csproj,*.slnx,*.json,*.props,*.http,*.md |
  ForEach-Object { (Get-Content $_ -Raw).Replace($old,$new) | Set-Content $_ -Encoding utf8 }
# Klasör ve dosya adları (uzundan kısaya)
Get-ChildItem -Recurse -Directory  | Sort-Object FullName -Descending | Where-Object Name -match $old | Rename-Item -NewName { $_.Name.Replace($old,$new) }
Get-ChildItem -Recurse -File       | Where-Object Name -match $old | Rename-Item -NewName { $_.Name.Replace($old,$new) }
```
Ardından `dotnet build` ile doğrula. **Jwt:Secret** ve bağlantı dizesini de kendi değerlerinle güncelle.

---

## 🧠 Mimari Tercihler (neden böyle?)

- **Kendi hafif CQRS'imiz (MediatR yok):** MediatR ve AutoMapper ticari lisansa geçti.
  `ISender` + `IPipelineBehavior` ~50 satırla aynı işi görür; bağımlılık ve lisans riski yok.
- **Manuel mapping (AutoMapper yok):** Derleme-zamanı güvenli, hızlı, okunaklı (`ProductMapping.ToDto`).
- **Result pattern:** Beklenen hatalar (not found, conflict, validation) exception ile değil,
  `Result/Error` ile akar. Exception yalnızca gerçek beklenmedik durumlar içindir.
- **Repository + UnitOfWork:** Application persistence'tan soyutlanır; testlerde kolayca mock'lanır.
  Query tarafında performans için `Query()` (AsNoTracking) + EF projeksiyonu kullanılır.
- **IExceptionHandler:** .NET 8+ ile gelen modern, merkezi hata yönetimi; çıktı RFC 7807 ProblemDetails.
- **Interceptor ile audit + soft delete:** `CreatedAt/UpdatedAt` ve silme işlemleri tek yerden, otomatik.
- **Central Package Management:** Tüm sürümler `Directory.Packages.props` içinde tek noktada.
- **Vertical Slice (Features/):** Bir özellik için gereken her şey tek klasörde; büyüdükçe karışmaz.

---

## 🧪 Test

```bash
dotnet test
```

`tests/KiraathaneTactics.Application.UnitTests` — handler'ları NSubstitute ile izole test etme örneği içerir.
İleride `*.IntegrationTests` (WebApplicationFactory) projesi eklenebilir; `Program` bunun için `partial`.

---

## ✅ Production'a Geçmeden Önce

- [ ] `Jwt:Secret` değerini güçlü ve gizli bir değerle değiştir (User Secrets / Key Vault / env).
- [ ] Gerçek bağlantı dizesini güvenli şekilde sağla (`ConnectionStrings__DefaultConnection`).
- [ ] `Program.cs` içindeki otomatik migrate/seed'i production için gözden geçir.
- [ ] CORS, rate limiting ve HTTPS/HSTS politikalarını ihtiyacına göre ekle.
