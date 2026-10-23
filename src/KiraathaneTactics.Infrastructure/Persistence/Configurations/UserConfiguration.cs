using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(40);
        builder.Property(u => u.RegisteredName).IsRequired().HasMaxLength(40).HasDefaultValue("");
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(u => u.Email).IsUnique();

        // Soft delete: silinmiş kullanıcılar sorgulardan otomatik çıkar
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
