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
using P3D.Legacy.Server.Services.Server;

using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            services.AddTransient<IRequestHandler<PlayerFinalizingCommand, Unit>, PlayerFinalizingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerInitializingCommand, Unit>, PlayerInitializingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerReadyCommand, Unit>, PlayerReadyCommandHandler>();


            services.AddTransient(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>());
            services.AddTransient(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>());
            services.AddTransient(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGameDataNotification>>());
            services.AddTransient(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            services.AddTransient(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>());

            services.AddTransient<INotificationHandler<PlayerSentGlobalMessageNotification>>(sp => sp.GetRequiredService<DiscordPassthroughService>());

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}