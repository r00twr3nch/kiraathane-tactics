namespace KiraathaneTactics.Domain.Common;

/// <summary>
/// Oluşturulma/güncellenme bilgisi tutulan entity'ler bu arayüzü uygular.
/// Bu alanlar EF Core interceptor'ı (AuditableEntityInterceptor) tarafından otomatik doldurulur.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
    string? UpdatedBy { get; set; }
}
