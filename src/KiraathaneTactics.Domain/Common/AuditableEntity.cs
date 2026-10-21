namespace KiraathaneTactics.Domain.Common;

/// <summary>
/// Hem denetim (audit) hem soft-delete alanlarını taşıyan hazır temel sınıf.
/// İş entity'lerin çoğu bundan türeyebilir.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
