using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.CommandHandlers;
using P3D.Legacy.Server.Commands;
using P3D.Legacy.Server.Notifications;
using P3D.Legacy.Server.Services;
using P3D.Legacy.Server.Services.Discord;

using System;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private class NotificationEnumerableContainer
        {
            private readonly Dictionary<Type, List<Func<IServiceProvider, IEnumerable<INotificationHandler<INotification>>>>> _notifications = new();

            public void AddNotificationEnumerable<TNotification>(Func<IServiceProvider, INotificationHandler<TNotification>> implementationFactory)
                where TNotification : INotification
            {
                if (!_notifications.TryGetValue(typeof(IEnumerable<INotificationHandler<TNotification>>), out var list))
                {
                    list = new();
                    _notifications.Add(typeof(IEnumerable<INotificationHandler<TNotification>>), list);
                }
                list.Add(sp => new[] { implementationFactory(sp) }.Cast<INotificationHandler<INotification>>());
            }

            public void AddNotificationEnumerable<TNotification>(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> implementationFactory)
                where TNotification : INotification
            {
                if (!_notifications.TryGetValue(typeof(IEnumerable<INotificationHandler<TNotification>>), out var list))
                {
                    list = new();
                    _notifications.Add(typeof(IEnumerable<INotificationHandler<TNotification>>), list);
                }
                list.Add(sp => implementationFactory(sp).Cast<INotificationHandler<INotification>>());
            }

            public IServiceCollection RegisterServices(IServiceCollection services)
            {
                foreach (var (type, funcs) in _notifications)
                {
                    services.AddTransient(type, sp =>
                    {
                        var list = funcs.SelectMany(func =>
                        {
                            var value = func(sp).ToList();

                            return value;
                        });
                        return typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(type).Invoke(null, new object[] { list });
                    });
                }

                return services;
            }
        }


        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            var nc = new NotificationEnumerableContainer();

            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            services.AddTransient<IRequestHandler<PlayerFinalizingCommand, Unit>, PlayerFinalizingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerInitializingCommand, Unit>, PlayerInitializingCommandHandler>();
            services.AddTransient<IRequestHandler<PlayerReadyCommand, Unit>, PlayerReadyCommandHandler>();

            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>());
            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>());
            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGameDataNotification>>());
            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>());

            nc.AddNotificationEnumerable(sp => sp.GetRequiredService<DiscordPassthroughService>() as INotificationHandler<PlayerSentGlobalMessageNotification>);

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            nc.RegisterServices(services);
            return services;
        }
    }
}