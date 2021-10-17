using MediatR;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Application.Notifications;
using P3D.Legacy.Server.GameCommands.CommandManagers;
using P3D.Legacy.Server.GameCommands.CommandManagers.Chat;
using P3D.Legacy.Server.GameCommands.CommandManagers.Client;
using P3D.Legacy.Server.GameCommands.CommandManagers.Permission;
using P3D.Legacy.Server.GameCommands.CommandManagers.World;
using P3D.Legacy.Server.GameCommands.NotificationHandlers;

using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.GameCommands.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IEnumerable<(Type, Type)> AddGameCommandsNotifications()
        {
            yield return (typeof(INotificationHandler<PlayerSentCommandNotification>), typeof(CommandManagerHandler));
        }

        public static IServiceCollection AddGameCommands(this IServiceCollection services)
        {
            services.AddTransient<CommandManager, HelpCommandManager>();

            //services.AddTransient<CommandManager, ChatChannelChangeCommandManager>();
            //services.AddTransient<CommandManager, ChatChannelInfoCommandManager>();
            //services.AddTransient<CommandManager, ChatChannelListCommandManager>();
            services.AddTransient<CommandManager, SayCommandManager>();

            services.AddTransient<CommandManager, BanCommandManager>();
            services.AddTransient<CommandManager, KickCommandManager>();
            services.AddTransient<CommandManager, MuteCommandManager>();
            services.AddTransient<CommandManager, UnbanCommandManager>();
            services.AddTransient<CommandManager, UnmuteCommandManager>();

            services.AddTransient<CommandManager, ShowPermissionsCommandManager>();

            services.AddTransient<CommandManager, SetSeasonCommandManager>();
            services.AddTransient<CommandManager, SetTimeCommandManager>();
            services.AddTransient<CommandManager, SetWeatherCommandManager>();

            services.AddSingleton<CommandManagerHandler>();

            return services;
        }
    }
}