using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class WallEntryConfiguration : IEntityTypeConfiguration<WallEntry>
{
    public void Configure(EntityTypeBuilder<WallEntry> builder)
    {
        builder.ToTable("WallEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Name).HasMaxLength(24).IsRequired();
        builder.HasIndex(entry => new { entry.MatchId, entry.Name }).IsUnique();
        builder.HasIndex(entry => new { entry.MatchId, entry.UserId }).IsUnique();
        builder.HasIndex(entry => entry.SignedAt);
    }
}
