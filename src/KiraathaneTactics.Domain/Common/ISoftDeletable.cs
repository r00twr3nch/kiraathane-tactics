namespace KiraathaneTactics.Domain.Common;

/// <summary>
/// Kayıt fiziksel olarak silinmez; IsDeleted = true yapılır (soft delete).
/// DbContext global query filter ile silinmiş kayıtlar sorgulardan otomatik çıkarılır.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
