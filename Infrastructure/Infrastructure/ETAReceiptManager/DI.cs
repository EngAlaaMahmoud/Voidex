using Application.Common.Services.ETAReceiptManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ETAReceiptManager;

public static class DI
{   

    public static IServiceCollection AddDirectETAIntegration(this IServiceCollection services)
    {
        // Register HttpClient for ETA API calls
        services.AddHttpClient<DirectETAIntegrationService>();
        
        // Register the Direct ETA Integration Service
        services.AddScoped<IDirectETAIntegration, DirectETAIntegrationService>();

        return services;
    }
} 