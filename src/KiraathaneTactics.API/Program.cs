using System.Diagnostics;
using KiraathaneTactics.API;
using KiraathaneTactics.API.Hubs;
using KiraathaneTactics.Application;
using KiraathaneTactics.Infrastructure;
using KiraathaneTactics.Infrastructure.Persistence;
using Serilog;

// --- 1) Bootstrap logger: DI kurulmadan önceki hataları da yakalamak için ---
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u appsettings.json'dan yapılandır
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // --- 2) Katmanların DI kayıtları (her katman kendi kayıtlarını yapar) ---
    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddApi();

    var app = builder.Build();

    // --- 3) HTTP pipeline ---
    app.UseExceptionHandler(); // GlobalExceptionHandler devreye girer

    // Swagger: PRODUCTION (canlı) HARİÇ tüm ortamlarda (Development + Staging/Test).
    // Canlıda API dokümantasyonu dışarı açılmaz.
    if (!app.Environment.IsProduction())
    {
        app.UseSwagger();
        app.UseSwaggerUI(o => o.SwaggerEndpoint("/swagger/v1/swagger.json", "Kıraathane Tactics API v1"));

        app.MapGet("/play", () => Results.Redirect("/play/index.html")).ExcludeFromDescription();
    }

    // Otomatik migrate + seed: YALNIZCA Development.
    // Test/Canlı ortamlarda migration'lar CI/CD ile (dotnet ef database update) uygulanır.
    if (app.Environment.IsDevelopment())
    {
        await app.InitializeDatabaseAsync();

        // Development'ta uygulama açılınca tarayıcıyı otomatik Swagger'da aç.
        // ('dotnet run' tarayıcıyı kendisi açmaz; bu kod nasıl başlatırsan başlat açar.)
        // İstemiyorsan bu bloğu silmen yeterli.
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var url = app.Urls.FirstOrDefault(u => u.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                      ?? app.Urls.FirstOrDefault()
                      ?? "https://localhost:7060";

            var swaggerUrl = $"{url}/swagger";
            Log.Information("Swagger arayüzü: {SwaggerUrl}", swaggerUrl);

            try
            {
                Process.Start(new ProcessStartInfo(swaggerUrl) { UseShellExecute = true });
            }
            catch
            {
                Log.Information("Tarayıcı otomatik açılamadı. Adresi elle aç: {SwaggerUrl}", swaggerUrl);
            }
        });
    }

    app.UseSerilogRequestLogging(); // her HTTP isteği için özet log
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<MatchHub>("/hubs/matches");

    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Uygulama beklenmedik şekilde sonlandı.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

// Integration test projelerinin WebApplicationFactory ile erişebilmesi için
public partial class Program;
