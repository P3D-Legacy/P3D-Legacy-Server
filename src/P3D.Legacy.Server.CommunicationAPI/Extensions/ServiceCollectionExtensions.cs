using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CommunicationAPI.Controllers;
using P3D.Legacy.Server.CommunicationAPI.Services;
using P3D.Legacy.Server.CQERS.Extensions;

namespace P3D.Legacy.Server.CommunicationAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommunicationAPI(this IServiceCollection services)
        {
            services.AddTransient<CommunicationController>();

            services.AddScoped<WebSocketHandlerFactory>();
            services.AddEventHandlers(static sp => sp.GetRequiredService<WebSocketSubscribtionManager>().Values);

            services.AddSingleton<WebSocketSubscribtionManager>();

            services.AddTransient<CommunicationController>();
            services.AddTransient<ServerV1Controller>();
            services.AddTransient<ServerV2Controller>();

            return services;
        }
    }
}