using BetterHostedServices;

using Discord;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Extensions;
using P3D.Legacy.Server.DiscordBot.BackgroundServices;

namespace P3D.Legacy.Server.DiscordBot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedServiceAsSingleton<DiscordPassthroughService>();
            services.AddNotifications(sp => sp.GetRequiredService<DiscordPassthroughService>());

            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<IDiscordClient, DiscordSocketClient>(sp => sp.GetRequiredService<DiscordSocketClient>());

            return services;
        }
    }
}