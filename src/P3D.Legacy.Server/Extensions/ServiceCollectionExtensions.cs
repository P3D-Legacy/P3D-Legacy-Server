using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.CommandHandlers;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Services;
using P3D.Legacy.Server.Services.Discord;
using P3D.Legacy.Server.Utils;

using System.Linq;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            new RequestRegistrar
            {
                { typeof(IRequestHandler<PlayerFinalizingCommand, Unit>), typeof(PlayerFinalizingCommandHandler) },
                { typeof(IRequestHandler<PlayerInitializingCommand, Unit>), typeof(PlayerInitializingCommandHandler) },
                { typeof(IRequestHandler<PlayerReadyCommand, Unit>), typeof(PlayerReadyCommandHandler) },
            }.Register(services);

            new NotificationRegistrar
            {
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGameDataNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>() },
                { sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>() },

                //{ sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerSentGlobalMessageNotification> },
                { typeof(INotificationHandler<PlayerSentGlobalMessageNotification>), typeof(DiscordPassthroughService) },
            }.Register(services);

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}