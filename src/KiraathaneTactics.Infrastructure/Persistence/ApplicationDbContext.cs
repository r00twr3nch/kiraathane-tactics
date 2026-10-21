using System.Reflection;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KiraathaneTactics.Infrastructure.Persistence;

/// <summary>
/// Uygulamanın EF Core veritabanı bağlamı.
/// IUnitOfWork'i de uygular: handler'lar SaveChanges için soyutlamaya (IUnitOfWork) bağımlıdır.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<TileDefinition> TileDefinitions => Set<TileDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aynı assembly'deki tüm IEntityTypeConfiguration<> sınıflarını otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
