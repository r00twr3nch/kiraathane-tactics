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
                await EnsureMatchVersusBotColumnAsync(cancellationToken);
                await EnsureWallTableAsync(cancellationToken);
                await EnsureRegisteredNameColumnAsync(cancellationToken);
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

    /// <summary>EnsureCreated mevcut dosyaya VersusBot eklemez. Çaycı masaları bu sütunla duvardan düşer.</summary>
    private async Task EnsureMatchVersusBotColumnAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await AddMatchColumnIfMissingAsync(connection, "VersusBot", """INTEGER NOT NULL DEFAULT 0""", cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    /// <summary>Duvar tablosu sonradan geldi. Mevcut SQLite dosyası migration uygulamaz.</summary>
    private async Task EnsureWallTableAsync(CancellationToken cancellationToken)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS "WallEntries" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_WallEntries" PRIMARY KEY,
                "MatchId" TEXT NOT NULL,
                "Name" TEXT NOT NULL,
                "Wins" INTEGER NOT NULL,
                "Draws" INTEGER NOT NULL,
                "Losses" INTEGER NOT NULL,
                "SignedAt" TEXT NOT NULL
            );
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_WallEntries_MatchId_Name" ON "WallEntries" ("MatchId", "Name");
            CREATE INDEX IF NOT EXISTS "IX_WallEntries_SignedAt" ON "WallEntries" ("SignedAt");
            """,
            cancellationToken);
        await AddWallColumnIfMissingAsync("UserId", "TEXT NULL", cancellationToken);
        await context.Database.ExecuteSqlRawAsync(
            """CREATE UNIQUE INDEX IF NOT EXISTS "IX_WallEntries_MatchId_UserId" ON "WallEntries" ("MatchId", "UserId");""",
            cancellationToken);
    }

    /// <summary>İlk kapı adı. Eski hesaplarda o anki duvar adından bir kez doldurulur.</summary>
    private async Task EnsureRegisteredNameColumnAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Users') WHERE name = 'RegisteredName'";
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
            if (!exists)
            {
                await context.Database.ExecuteSqlRawAsync(
                    """ALTER TABLE "Users" ADD COLUMN "RegisteredName" TEXT NOT NULL DEFAULT '';""",
                    cancellationToken);
            }

            await context.Database.ExecuteSqlRawAsync(
                """UPDATE "Users" SET "RegisteredName" = "FullName" WHERE "RegisteredName" = '';""",
                cancellationToken);
            // Eski kayıtlarda aynı ad birden fazla olabilir. İlk hesap kalır, sonrakiler ayrılır.
            await context.Database.ExecuteSqlRawAsync(
                """
                UPDATE "Users"
                SET "RegisteredName" = substr("RegisteredName", 1, 33) || '-' || substr("Id", 1, 6)
                WHERE "Id" NOT IN (
                    SELECT MIN("Id") FROM "Users" GROUP BY lower("RegisteredName")
                );
                """,
                cancellationToken);
            await context.Database.ExecuteSqlRawAsync(
                """CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_RegisteredName" ON "Users" ("RegisteredName");""",
                cancellationToken);
        }
        finally
        {
            if (!wasOpen)
                await connection.CloseAsync();
        }
    }

    private async Task AddWallColumnIfMissingAsync(
        string column,
        string definition,
        CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await connection.OpenAsync(cancellationToken);

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('WallEntries') WHERE name = $name";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = column;
            command.Parameters.Add(parameter);
            var exists = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken)) > 0;
            if (exists)
                return;

            await context.Database.ExecuteSqlRawAsync(
                $"""ALTER TABLE "WallEntries" ADD COLUMN "{column}" {definition};""",
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
            RegisteredName = "System Admin",
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
            await AddMissingTilesAsync(cancellationToken);
            return;
        }

        logger.LogInformation("Kıraathane taş kataloğu oluşturuluyor...");

        context.TileDefinitions.AddRange(CatalogTiles());
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// On dört fincan, elde iki tam sıra. Kenarlar aynı kuralı konuşur; ortadaki sayı yalnız beraberlikte ağırlık olur.
    /// Kör Fincan kimseyi alamaz. Hançer tek yöne yüklenir. Tepsi dört yana da orta bakar.
    /// Kama iki karşı kenarı keser. Nargile Çekimi doğuya yaslanır.
    /// </summary>
    private static TileDefinition[] CatalogTiles() =>
    [
        new("Çay", 3, TileEdges.Create(2, 4, 1, 3)),
        new("Kahve", 3, TileEdges.Create(6, 2, 4, 1)),
        new("Nargile", 3, TileEdges.Create(1, 7, 2, 3)),
        new("Tavla", 4, TileEdges.Create(5, 3, 6, 2)),
        new("Gazete", 3, TileEdges.Create(3, 1, 2, 5)),
        new("Lokum", 1, TileEdges.Create(1, 2, 1, 1)),
        new("Satranç", 7, TileEdges.Create(7, 5, 4, 6)),
        new("Boş Fincan", 2, TileEdges.Create(0, 3, 4, 2)),
        new("Kör Fincan", 4, TileEdges.Create(0, 0, 0, 0)),
        new("Hançer", 2, TileEdges.Create(9, 0, 0, 1)),
        new("Tepsi", 5, TileEdges.Create(4, 4, 4, 4)),
        new("Zar", 6, TileEdges.Create(8, 1, 3, 2)),
        new("Kama", 3, TileEdges.Create(6, 0, 6, 0)),
        new("Çekim", 4, TileEdges.Create(1, 8, 2, 0))
    ];

    /// <summary>Eski veritabanına sonradan eklenen fincanları isimle tamamlar. Var olana dokunmaz.</summary>
    private async Task AddMissingTilesAsync(CancellationToken cancellationToken)
    {
        var names = await context.TileDefinitions
            .Select(tile => tile.Name)
            .ToListAsync(cancellationToken);
        var known = new HashSet<string>(names, StringComparer.Ordinal);
        var missing = CatalogTiles().Where(tile => !known.Contains(tile.Name)).ToArray();
        if (missing.Length == 0)
            return;

        logger.LogInformation("Kataloga {Count} yeni fincan ekleniyor.", missing.Length);
        context.TileDefinitions.AddRange(missing);
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
            ["Boş Fincan"] = 2,
            ["Kör Fincan"] = 4,
            ["Hançer"] = 2,
            ["Tepsi"] = 5,
            ["Zar"] = 6,
            ["Kama"] = 3,
            ["Çekim"] = 4
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
