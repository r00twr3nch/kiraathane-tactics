using System.Security.Claims;
using System.Text;
using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Models;
using KiraathaneTactics.Infrastructure.Authentication;
using KiraathaneTactics.Infrastructure.Persistence;
using KiraathaneTactics.Infrastructure.Persistence.Interceptors;
using KiraathaneTactics.Infrastructure.Persistence.Repositories;
using KiraathaneTactics.Infrastructure.Security;
using KiraathaneTactics.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace KiraathaneTactics.Infrastructure;

/// <summary>
/// Infrastructure katmanının DI kayıtları. Program.cs'ten: AddInfrastructure(configuration).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddJwtAuthentication(configuration);

        // Servisler
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("'DefaultConnection' connection string'i bulunamadı.");

        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase)
                || connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }

            options.AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>());
        });

        // IUnitOfWork olarak DbContext'in kendisi kullanılır
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Repository'ler
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        // Açılışta migrate + seed için
        services.AddScoped<ApplicationDbContextInitializer>();

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        var settings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("'Jwt' yapılandırma bölümü bulunamadı.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier
                };

                // 401/403 yanıtlarını da tek tip ApiResponse zarfına çevir (mobil/web tutarlılığı).
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse(); // varsayılan boş 401 gövdesini bastır
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                            StatusCodes.Status401Unauthorized,
                            "Kimlik doğrulaması gerekli.",
                            [new ApiError("Auth.Unauthorized", null, "Geçerli bir token sağlanmadı veya süresi doldu.")]));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(
                            StatusCodes.Status403Forbidden,
                            "Bu işlem için yetkiniz yok.",
                            [new ApiError("Auth.Forbidden", null, "Gerekli role/izne sahip değilsiniz.")]));
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
