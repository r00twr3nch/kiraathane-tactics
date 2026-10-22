using System.Text.Json;
using System.Text.Json.Serialization;
using KiraathaneTactics.API.Middleware;
using KiraathaneTactics.API.Services;
using KiraathaneTactics.Application.Common.Interfaces;
using Microsoft.OpenApi.Models;

namespace KiraathaneTactics.API;

/// <summary>
/// API (presentation) katmanının DI kayıtları: controller'lar, Swagger, ProblemDetails,
/// global exception handler ve web'e özel servisler.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // Tahta "Waiting" / "Light" bekler. Sayı (0, 1) lobi ve sıra yazısını bozar.
        // Aynı seçenekler HTTP yanıtı, istek gövdesi ve SignalR "state" yayını için geçerlidir.
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddControllers()
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSignalR(options =>
            {
                // Sessiz kopuş varsayılan 30 saniyede fark edilir. Kapıyı çeken
                // rakip bu kadar bekletilmez; yeniden bağlanma mühleti hub'da ayrıca durur.
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddScoped<IMatchNotifier, SignalRMatchNotifier>();
        services.AddSingleton<IOnlineQueue, OnlineQueue>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Tek noktadan exception yönetimi (tüm hatalar ApiResponse zarfına çevrilir).
        // AddProblemDetails, UseExceptionHandler() middleware'inin teknik gereksinimidir
        // (fallback); pratikte GlobalExceptionHandler her hatayı işlediği için kullanılmaz.
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerWithJwt();

        return services;
    }

    private static void AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "KiraathaneTactics API",
                Version = "v1",
                Description = "Clean Architecture + CQRS tabanlı .NET 10 API şablonu."
            });

            // Tüm projelerin XML dokümantasyon dosyalarını dahil et
            // (controller özetleri + Command/Query/DTO alan açıklamaları + örnek değerler).
            foreach (var xmlFile in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);

            // Swagger UI'da "Authorize" butonu ile JWT girilebilsin
            var jwtScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT token'ı şu formatta girin: Bearer {token}",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { jwtScheme, Array.Empty<string>() }
            });
        });
    }
}
