using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.CommandHandlers;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Services.Discord;
using P3D.Legacy.Server.Services.Server;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration());

            services.AddTransient<IRequestHandler<PlayerFinalizingCommand, Unit>, PlayerFinalizingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerInitializingCommand, Unit>, PlayerInitializingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerReadyCommand, Unit>, PlayerReadyCommandHandler>();


            services.AddScoped<INotificationHandler<PlayerJoinedNotification>>(sp => sp.GetRequiredService<P3DConnectionContextHandler>());
            services.AddScoped<INotificationHandler<PlayerLeavedNotification>>(sp => sp.GetRequiredService<P3DConnectionContextHandler>());
            services.AddScoped<INotificationHandler<PlayerSentGameDataNotification>>(sp => sp.GetRequiredService<P3DConnectionContextHandler>());
            services.AddScoped<INotificationHandler<PlayerSentGlobalMessageNotification>>(sp => sp.GetRequiredService<P3DConnectionContextHandler>());
            services.AddScoped<INotificationHandler<PlayerSentLocalMessageNotification>>(sp => sp.GetRequiredService<P3DConnectionContextHandler>());

            services.AddTransient<INotificationHandler<PlayerSentGlobalMessageNotification>>(sp => sp.GetRequiredService<DiscordPassthroughService>());

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}