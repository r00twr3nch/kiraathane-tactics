using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class BoardCellConfiguration : IEntityTypeConfiguration<BoardCell>
{
    public void Configure(EntityTypeBuilder<BoardCell> builder)
    {
        builder.ToTable("BoardCells");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Row).IsRequired();
        builder.Property(c => c.Column).IsRequired();
        builder.Property(c => c.Owner).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.PlacedBy).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.Value).IsRequired();
        builder.Property(c => c.North).IsRequired();
        builder.Property(c => c.East).IsRequired();
        builder.Property(c => c.South).IsRequired();
        builder.Property(c => c.West).IsRequired();

        builder.HasIndex(c => new { c.MatchId, c.Row, c.Column }).IsUnique();

        builder.HasOne<TileDefinition>()
            .WithMany()
            .HasForeignKey(c => c.TileDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
