using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KiraathaneTactics.Infrastructure.Persistence;

/// <summary>
/// Yalnızca TASARIM ZAMANINDA kullanılır (dotnet ef migrations add / database update).
/// Uygulamayı çalıştırmadan DbContext'i oluşturabilmek için EF Core tooling bunu kullanır.
/// Buradaki bağlantı dizesi yalnızca migration üretmek/uygulamak içindir; çalışma zamanında
/// gerçek bağlantı appsettings.json'dan (DI üzerinden) gelir.
/// </summary>
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // İstersen ortam değişkeniyle ezebilirsin: ConnectionStrings__DefaultConnection
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=KiraathaneTacticsDb;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new ApplicationDbContext(options);
    }
}
