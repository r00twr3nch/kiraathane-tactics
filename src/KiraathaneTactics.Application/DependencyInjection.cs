using System.Reflection;
using KiraathaneTactics.Application.Common.Behaviors;
using KiraathaneTactics.Application.Common.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace KiraathaneTactics.Application;

/// <summary>
/// Application katmanının DI kayıtları. Program.cs'ten tek satırla çağrılır: AddApplication().
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Dağıtıcı
        services.AddScoped<ISender, Sender>();

        // Tüm IRequestHandler<,> implementasyonlarını otomatik kaydet
        RegisterHandlers(services, assembly);

        // Pipeline behavior'lar — KAYIT SIRASI ÖNEMLİ (ilk kayıt = en dış halka).
        // Logging en dışta (her şeyi sarar), Validation handler'a en yakın.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // FluentValidation: bu assembly'deki tüm validator'ları kaydet
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var registrations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => new { Service = i, Implementation = t }));

        foreach (var reg in registrations)
            services.AddScoped(reg.Service, reg.Implementation);
    }
}
