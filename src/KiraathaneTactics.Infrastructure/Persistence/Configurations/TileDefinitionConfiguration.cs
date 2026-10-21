using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KiraathaneTactics.Infrastructure.Persistence.Configurations;

public sealed class TileDefinitionConfiguration : IEntityTypeConfiguration<TileDefinition>
{
    public void Configure(EntityTypeBuilder<TileDefinition> builder)
    {
        builder.ToTable("TileDefinitions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(80);
        builder.Property(t => t.Value).IsRequired();
        builder.Ignore(t => t.Edges);

        builder.HasIndex(t => t.Name);
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
