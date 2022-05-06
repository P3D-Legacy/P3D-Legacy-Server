using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Extensions;
using P3D.Legacy.Server.GameCommands.CommandManagers;
using P3D.Legacy.Server.GameCommands.CommandManagers.Chat;
using P3D.Legacy.Server.GameCommands.CommandManagers.Permission;
using P3D.Legacy.Server.GameCommands.CommandManagers.Player;
using P3D.Legacy.Server.GameCommands.CommandManagers.World;
using P3D.Legacy.Server.GameCommands.EventHandlers;

namespace P3D.Legacy.Server.GameCommands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGameCommands(this IServiceCollection services)
        {
            services.AddTransient<CommandManagerHandler>();
            services.AddEvent(static sp => sp.GetRequiredService<CommandManagerHandler>());

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

            services.AddTransient<CommandManager, TriggerPlayerEventCommandManager>();

            services.AddSingleton<CommandManagerHandler>();

            return services;
        }
    }
}