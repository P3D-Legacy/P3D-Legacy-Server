using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.GameCommands.CommandManagers;
using P3D.Legacy.Server.GameCommands.CommandManagers.Chat;
using P3D.Legacy.Server.GameCommands.CommandManagers.Permission;
using P3D.Legacy.Server.GameCommands.CommandManagers.Player;
using P3D.Legacy.Server.GameCommands.CommandManagers.World;
using P3D.Legacy.Server.GameCommands.NotificationHandlers;

namespace P3D.Legacy.Server.GameCommands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameCommandsMediatR(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            notificationRegistrar.Add(sp => sp.GetRequiredService<CommandManagerHandler>() as INotificationHandler<PlayerSentCommandNotification>);

            return services;
        }

        public static IServiceCollection AddGameCommands(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<CommandManagerHandler>();

            services.AddTransient<CommandManager, HelpCommandManager>();

            //services.AddTransient<CommandManager, ChatChannelChangeCommandManager>();
            //services.AddTransient<CommandManager, ChatChannelInfoCommandManager>();
            //services.AddTransient<CommandManager, ChatChannelListCommandManager>();
            services.AddTransient<CommandManager, SayCommandManager>();

            services.AddTransient<CommandManager, BanCommandManager>();
            services.AddTransient<CommandManager, GetGameJoltIdCommandManager>();
            services.AddTransient<CommandManager, KickCommandManager>();
            services.AddTransient<CommandManager, LoginCommandManager>();
            services.AddTransient<CommandManager, MuteCommandManager>();
            services.AddTransient<CommandManager, UnbanCommandManager>();
            services.AddTransient<CommandManager, UnmuteCommandManager>();

            services.AddTransient<CommandManager, SetPermissionCommand>();
            services.AddTransient<CommandManager, ShowPermissionsCommandManager>();

            services.AddTransient<CommandManager, SetSeasonCommandManager>();
            services.AddTransient<CommandManager, SetTimeCommandManager>();
            services.AddTransient<CommandManager, SetWeatherCommandManager>();

            services.AddSingleton<CommandManagerHandler>();

            return services;
        }
    }
}