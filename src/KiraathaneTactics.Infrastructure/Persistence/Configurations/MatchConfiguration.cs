using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("Matches");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.CurrentTurn).HasConversion<string>().HasMaxLength(10);
        builder.Property(m => m.Winner).HasConversion<string>().HasMaxLength(10);
                builder.Property(m => m.RoomCode).HasMaxLength(6).IsRequired();
                builder.HasIndex(m => m.RoomCode).IsUnique();
                builder.Property(m => m.LightSeatToken).HasMaxLength(64);
                builder.Property(m => m.DarkSeatToken).HasMaxLength(64);
                builder.Property(m => m.Walkover).HasDefaultValue(false);
                builder.Property(m => m.VersusBot).HasDefaultValue(false);

                builder.Ignore(m => m.Cells);
        builder.Ignore(m => m.DomainEvents);

        builder.HasMany(m => m.CellsForPersistence)
                    .WithOne()
                    .HasForeignKey(c => c.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.Navigation(m => m.CellsForPersistence)
                    .HasField("_cells")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}
