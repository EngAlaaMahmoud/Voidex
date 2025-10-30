using Application.Common.Services.EmailManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ETA;

public static class DI
{
    public static IServiceCollection RegisterETAReciptToolkitManager(this IServiceCollection services, IConfiguration configuration)
    {
      //  services.AddSingleton<IToolkitHandler, ToolkitHandler>();
        return services;
    }
}
