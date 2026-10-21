# Neyi Nereye Koyarım? — Başlangıç Rehberi

Sıfırdan başlayan biri için en sık sorulan soru: **"Bunu hangi klasöre, hangi dosyaya yazacağım?"**
Bu rehber tam olarak onu anlatır. Önce hızlı harita, sonra **uçtan uca çalışan bir örnek** var.

---

## 1) Altın Kurallar (önce bunları aklında tut)

1. **Bağımlılık hep içe akar:** `API → Infrastructure → Application → Domain`. İç katman dışı **bilmez**.
2. **Domain** hiçbir şeye bağımlı değildir (ne EF, ne HTTP, ne JWT). Sadece saf iş nesneleri.
3. **Application** "ne yapılacak"ı bilir ama "nasıl"ı bilmez → altyapıya **interface** ile bakar.
4. **Infrastructure** o interface'lerin somut halidir (EF, JWT, BCrypt...).
5. **API** sadece isteği alır, CQRS'e (`Sender.Send`) devreder, sonucu HTTP'ye çevirir. **İş mantığı içermez.**
6. Her özellik kendi klasöründe yaşar (**vertical slice**) → `Features/{Özellik}/{Aksiyon}/`.

---

## 2) Hızlı Harita — "X eklemek istiyorum, nereye?"

| Eklemek istediğim | Klasör / Dosya | Canlı örnek |
|---|---|---|
| Yeni tablo / iş nesnesi (entity) | `Domain/Entities/X.cs` | `Product.cs` |
| Enum | `Domain/Enums/X.cs` | `UserRole.cs` |
| İş kuralı / domain hatası | Entity'nin içinde + `Domain/Exceptions` | `Product.SetPrice()` → `DomainException` |
| Yazma use-case'i (Command) | `Application/Features/X/Create/` → Command + Handler + Validator | `CreateProductCommand` |
| Okuma use-case'i (Query) | `Application/Features/X/GetList/` → Query + Handler | `GetProductsQuery` |
| Dışarı dönen model (DTO) | `Application/Features/X/Common/XDto.cs` | `ProductDto` |
| Feature'a özel hata kodları | `Application/Features/X/Common/XErrors.cs` | `ProductErrors` |
| Doğrulama kuralı | İlgili `...CommandValidator.cs` | `CreateProductCommandValidator` |
| Soyut servis (sözleşme) | `Application/Common/Interfaces/IX.cs` | `IJwtTokenGenerator` |
| Servisin gerçek kodu | `Infrastructure/{uygun klasör}/X.cs` | `JwtTokenGenerator` |
| EF tablo ayarı (kolon, index) | `Infrastructure/Persistence/Configurations/XConfiguration.cs` | `ProductConfiguration` |
| Özel repository | `Application/Common/Interfaces` (arayüz) + `Infrastructure/.../Repositories` (kod) | `IUserRepository` / `UserRepository` |
| HTTP ucu (endpoint) | `API/Controllers/XController.cs` | `ProductsController` |
| Tüm projede ortak model | `Application/Common/Models/X.cs` | `ApiResponse`, `PagedResult` |
| Çapraz-kesit davranış (log, validation...) | `Application/Common/Behaviors/X.cs` | `LoggingBehavior` |
| Yeni NuGet paketi sürümü | `Directory.Packages.props` | — |

---

## 3) UÇTAN UCA ÖRNEK: "Category" özelliğini sıfırdan ekleme

Diyelim ürünlere kategori ekleyeceğiz. Aşağıdaki **6 adım** tüm akışı gösterir.
(Aynı desen her yeni özellik için geçerlidir.)

### Adım 1 — Domain: entity'yi oluştur
📁 `src/KiraathaneTactics.Domain/Entities/Category.cs`
```csharp
using KiraathaneTactics.Domain.Common;

namespace KiraathaneTactics.Domain.Entities;

public class Category : AuditableEntity   // Id + CreatedAt/UpdatedAt + soft-delete hazır gelir
{
    public string Name { get; set; } = string.Empty;
}
```

### Adım 2 — Infrastructure: EF'e tanıt
📁 `src/KiraathaneTactics.Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`
```csharp
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.HasQueryFilter(c => !c.IsDeleted);   // soft-delete
    }
}
```
📁 `ApplicationDbContext.cs` içine bir satır ekle:
```csharp
public DbSet<Category> Categories => Set<Category>();
```
> Configuration sınıfı `ApplyConfigurationsFromAssembly` ile **otomatik** bulunur, ekstra kayıt yok.

### Adım 3 — Application: DTO + hata kodları
📁 `src/KiraathaneTactics.Application/Features/Categories/Common/CategoryDto.cs`
```csharp
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Categories.Common;

public sealed record CategoryDto(Guid Id, string Name);

public static class CategoryMapping
{
    public static CategoryDto ToDto(this Category c) => new(c.Id, c.Name);
}
```
📁 `.../Categories/Common/CategoryErrors.cs`
```csharp
using KiraathaneTactics.Application.Common.Results;

namespace KiraathaneTactics.Application.Features.Categories.Common;

public static class CategoryErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Category.NotFound", $"'{id}' kimlikli kategori bulunamadı.");
}
```

### Adım 4 — Application: yazma use-case'i (Command + Validator + Handler)
📁 `.../Categories/Create/CreateCategoryCommand.cs`
```csharp
using KiraathaneTactics.Application.Common.Messaging;

namespace KiraathaneTactics.Application.Features.Categories.Create;

public sealed record CreateCategoryCommand : ICommand<Guid>
{
    /// <summary>Kategori adı.</summary>
    /// <example>Elektronik</example>
    public string Name { get; init; } = string.Empty;
}
```
📁 `.../Categories/Create/CreateCategoryCommandValidator.cs`
```csharp
using FluentValidation;

namespace KiraathaneTactics.Application.Features.Categories.Create;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode("Category.Name.Required").WithMessage("Kategori adı zorunludur.")
            .MaximumLength(150).WithErrorCode("Category.Name.TooLong").WithMessage("En fazla 150 karakter.");
    }
}
```
📁 `.../Categories/Create/CreateCategoryCommandHandler.cs`
```csharp
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Categories.Create;

public sealed class CreateCategoryCommandHandler(
    IGenericRepository<Category> repository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category { Name = request.Name };
        await repository.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
```
> Handler ve Validator **otomatik** DI'a kaydolur (assembly tarama). `IGenericRepository<Category>` açık generic olarak zaten kayıtlı — ekstra hiçbir şey gerekmez.

### Adım 5 — API: endpoint
📁 `src/KiraathaneTactics.API/Controllers/CategoriesController.cs`
```csharp
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Application.Features.Categories.Create;
using Microsoft.AspNetCore.Mvc;

namespace KiraathaneTactics.API.Controllers;

public sealed class CategoriesController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>Yeni kategori oluşturur.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleCreated(result, nameof(Create), new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }
}
```

### Adım 6 — Migration ekle ve uygula
```bash
dotnet ef migrations add AddCategory \
  --project src/KiraathaneTactics.Infrastructure --startup-project src/KiraathaneTactics.API \
  --output-dir Persistence/Migrations
# Development'ta açılışta otomatik uygulanır; manuel için:
dotnet ef database update --project src/KiraathaneTactics.Infrastructure --startup-project src/KiraathaneTactics.API
```

✅ Bu kadar. Hiçbir DI dosyasına elle kayıt eklemedik — desen sayesinde her şey otomatik bağlandı.

---

## 4) Sık Sorulanlar

**"Dışarıdan API çağıracak bir servis (örn. e-posta gönderimi) eklemem lazım, nereye?"**
→ Sözleşmeyi `Application/Common/Interfaces/IEmailSender.cs`'e, gerçek kodu `Infrastructure/Services/EmailSender.cs`'e koy.
Sonra `Infrastructure/DependencyInjection.cs` → `services.AddScoped<IEmailSender, EmailSender>();`.

**"Handler'ın içinde HttpContext / o anki kullanıcı lazım."**
→ `ICurrentUserService`'i constructor'a enjekte et (zaten hazır). Handler asla doğrudan `HttpContext` görmez.

**"Aynı anda birden fazla tabloyu kaydedeceğim."**
→ Hepsini repository'lerle ekle/güncelle, **tek** `unitOfWork.SaveChangesAsync()` çağır → hepsi tek transaction.

**"Sadece bu entity'ye özel bir sorgu lazım (örn. isme göre)."**
→ `IGenericRepository` yetmezse `IXRepository : IGenericRepository<X>` arayüzü aç (Application),
implementasyonu `Infrastructure/.../Repositories`'e yaz, DI'a kaydet. Örnek: `IUserRepository`.

**"İş mantığını controller'a yazsam olmaz mı?"**
→ Olmaz. Controller sadece `Sender.Send(...)` çağırır. Mantık **Handler**'da yaşar (test edilebilirlik + tekrar kullanım).
