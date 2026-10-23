using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Domain.Entities;
using KiraathaneTactics.Domain.Enums;
using KiraathaneTactics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KiraathaneTactics.Infrastructure.Persistence;

/// <summary>
/// Uygulama açılışında bekleyen migration'ları uygular ve temel (seed) veriyi ekler.
/// </summary>
public sealed class ApplicationDbContextInitializer(
    ILogger<ApplicationDbContextInitializer> logger,
    ApplicationDbContext context,
    IPasswordHasher passwordHasher)
{
    public async Task MigrateAndSeedAsync(CancellationToken cancellationToken = default)
    {
        await MigrateAsync(cancellationToken);
        await SeedAsync(cancellationToken);
    }

    private async Task MigrateAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (context.Database.IsSqlite())
            {
                // SQL Server migration'ları bu makinede uygulanamaz: LocalDB ve her iki
                // Express hizmeti de başlamıyor. Geliştirme dosyası şemayı modelden kurar.
                logger.LogInformation("SQLite geliştirme veritabanı hazırlanıyor...");
                await context.Database.EnsureCreatedAsync(cancellationToken);
                await EnsureBoardCellWeightColumnAsync(cancellationToken);
                await EnsureBoardCellPlacedByColumnAsync(cancellationToken);
                await EnsureMatchWalkoverColumnAsync(cancellationToken);
                await EnsureMatchClockBankColumnsAsync(cancellationToken);
            }
            else if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                logger.LogInformation("Bekleyen migration'lar uygulanıyor...");
                await context.Database.MigrateAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı migration'ı sırasında hata oluştu.");
            throw;
        }
    }

    /// <summary>EnsureCreated mevcut dosyaya yeni sütun eklemez. Ağırlık sonradan geldi.</summary>
    private async Task EnsureBoardCellWeightColumnAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('BoardCells') WHERE name = 'Value'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
            if (exists)
                return;

            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "BoardCells" ADD COLUMN "Value" INTEGER NOT NULL DEFAULT 0;""",
                cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    /// <summary>Kopya sınırı koyanı hatırlar. Eski karelerde koyan, o anki sahip sayılır.</summary>
    private async Task EnsureBoardCellPlacedByColumnAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('BoardCells') WHERE name = 'PlacedBy'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
            if (exists)
                return;

            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "BoardCells" ADD COLUMN "PlacedBy" TEXT NULL;""",
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "BoardCells" SET "PlacedBy" = "Owner" WHERE "Owner" IS NOT NULL AND "PlacedBy" IS NULL;""",
                cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    /// <summary>Hükmen galibiyet notu. Eski masalarda false sayılır.</summary>
    private async Task EnsureMatchWalkoverColumnAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Matches') WHERE name = 'Walkover'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
            if (exists)
                return;

            await context.Database.ExecuteSqlRawAsync(
                """ALTER TABLE "Matches" ADD COLUMN "Walkover" INTEGER NOT NULL DEFAULT 0;""",
                cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    /// <summary>Satranç saati: her tarafın kendi bankası ve sıranın başladığı an.</summary>
    private async Task EnsureMatchClockBankColumnsAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            await AddMatchColumnIfMissingAsync(connection, "LightSecondsLeft", """INTEGER NOT NULL DEFAULT 210""", cancellationToken);
            await AddMatchColumnIfMissingAsync(connection, "DarkSecondsLeft", """INTEGER NOT NULL DEFAULT 210""", cancellationToken);
            await AddMatchColumnIfMissingAsync(connection, "TurnStartedAt", "TEXT NULL", cancellationToken);
            await AddMatchColumnIfMissingAsync(connection, "MatchLengthSeconds", """INTEGER NOT NULL DEFAULT 210""", cancellationToken);
            await AddMatchColumnIfMissingAsync(connection, "HostPlaysDark", """INTEGER NOT NULL DEFAULT 0""", cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "Matches" SET "TurnStartedAt" = "StartedAt" WHERE "TurnStartedAt" IS NULL AND "StartedAt" IS NOT NULL;""",
                cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private async Task AddMatchColumnIfMissingAsync(
        System.Data.Common.DbConnection connection,
        string column,
        string definition,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Matches') WHERE name = $name";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "$name";
        parameter.Value = column;
        command.Parameters.Add(parameter);
        var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
        if (exists)
            return;

        await context.Database.ExecuteSqlRawAsync(
            $"""ALTER TABLE "Matches" ADD COLUMN "{column}" {definition};""",
            cancellationToken);
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        await SeedAdminAsync(cancellationToken);
        await SeedTilesAsync(cancellationToken);
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        // Varsayılan admin kullanıcısı (yalnızca hiç kullanıcı yoksa)
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        logger.LogInformation("Varsayılan admin kullanıcısı oluşturuluyor...");

        var admin = new User
        {
            Email = "admin@kiraathane.local",
            FullName = "System Admin",
            Role = UserRole.Admin,
            PasswordHash = passwordHasher.Hash("Admin123!")
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedTilesAsync(CancellationToken cancellationToken)
    {
        if (await context.TileDefinitions.AnyAsync(cancellationToken))
        {
            await AlignCatalogWeightsAsync(cancellationToken);
            return;
        }

        logger.LogInformation("Kıraathane taş kataloğu oluşturuluyor...");

        // Ortadaki sayı fincanın ağırlığıdır: kenarların ortalamasına yakın, beraberlikte karar verir.
        TileDefinition[] catalog =
        [
            new("Çay", 3, TileEdges.Create(2, 4, 1, 3)),
            new("Kahve", 3, TileEdges.Create(6, 2, 4, 1)),
            new("Nargile", 3, TileEdges.Create(1, 7, 2, 3)),
            new("Tavla", 4, TileEdges.Create(5, 3, 6, 2)),
            new("Gazete", 3, TileEdges.Create(3, 1, 2, 5)),
            new("Lokum", 1, TileEdges.Create(1, 2, 1, 1)),
            new("Satranç", 7, TileEdges.Create(7, 5, 4, 6)),
            new("Boş Fincan", 2, TileEdges.Create(0, 3, 4, 2))
        ];

        context.TileDefinitions.AddRange(catalog);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Eski katalogdaki rastgele ağırlıkları kenar ortalamasına çeker. Kenarlara dokunmaz.</summary>
    private async Task AlignCatalogWeightsAsync(CancellationToken cancellationToken)
    {
        var weights = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["Çay"] = 3,
            ["Kahve"] = 3,
            ["Nargile"] = 3,
            ["Tavla"] = 4,
            ["Gazete"] = 3,
            ["Lokum"] = 1,
            ["Satranç"] = 7,
            ["Boş Fincan"] = 2
        };

        var tiles = await context.TileDefinitions.ToListAsync(cancellationToken);
        var changed = false;
        foreach (var tile in tiles)
        {
            if (!weights.TryGetValue(tile.Name, out var weight) || tile.Value == weight)
                continue;

            tile.SetWeight(weight);
            changed = true;
        }

        if (changed)
            await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>WebApplication/IHost üzerinden tek satırla çağrı: await app.InitializeDatabaseAsync();</summary>
public static class DatabaseInitializerExtensions
{
    public static async Task InitializeDatabaseAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        await initializer.MigrateAndSeedAsync();
    }
}
