using BetterHostedServices;

using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Abstractions.Utils;
using P3D.Legacy.Server.Application.CommandHandlers.Administration;
using P3D.Legacy.Server.Application.CommandHandlers.Player;
using P3D.Legacy.Server.Application.CommandHandlers.World;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Administration;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Commands.World;
using P3D.Legacy.Server.Application.Extensions;
using P3D.Legacy.Server.Application.Queries.Bans;
using P3D.Legacy.Server.Application.Queries.Permissions;
using P3D.Legacy.Server.Application.Queries.Players;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Application.Services.Connections;
using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.GameCommands.Extensions;
using P3D.Legacy.Server.Infrastructure.Extensions;
using P3D.Legacy.Server.Infrastructure.Monsters;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories;
using P3D.Legacy.Server.Options;
using P3D.Legacy.Server.Services.Server;
using P3D.Legacy.Server.Statistics.Extensions;

using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInternal(this IServiceCollection services, IConfiguration configuration, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            var validationEnabled = configuration["Server:ValidationEnabled"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            var isOfficial = configuration["Server:IsOfficial"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

            services.Configure<ServerOptions>(configuration.GetSection("Server"));
            services.Configure<P3DOptions>(configuration.GetSection("P3D"));
            services.Configure<LiteDbOptions>(configuration.GetSection("LiteDb"));

            services.AddTransient<ChangePlayerPermissionsCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangePlayerPermissionsCommandHandler>() as IRequestHandler<ChangePlayerPermissionsCommand, CommandResult>);
            services.AddTransient<PlayerFinalizingCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerFinalizingCommandHandler>() as IRequestHandler<PlayerFinalizingCommand>);
            services.AddTransient<PlayerInitializingCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerInitializingCommandHandler>() as IRequestHandler<PlayerInitializingCommand>);
            services.AddTransient<PlayerReadyCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerReadyCommandHandler>() as IRequestHandler<PlayerReadyCommand>);
            services.AddTransient<PlayerMutedPlayerCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerMutedPlayerCommandHandler>() as IRequestHandler<PlayerMutedPlayerCommand, CommandResult>);
            services.AddTransient<PlayerUnmutedPlayerCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<PlayerUnmutedPlayerCommandHandler>() as IRequestHandler<PlayerUnmutedPlayerCommand, CommandResult>);
            services.AddTransient<BanPlayerCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<BanPlayerCommandHandler>() as IRequestHandler<BanPlayerCommand, CommandResult>);
            services.AddTransient<UnbanPlayerCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<UnbanPlayerCommandHandler>() as IRequestHandler<UnbanPlayerCommand, CommandResult>);
            services.AddTransient<KickPlayerCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<KickPlayerCommandHandler>() as IRequestHandler<KickPlayerCommand, CommandResult>);
            services.AddTransient<ChangeWorldSeasonCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldSeasonCommandHandler>() as IRequestHandler<ChangeWorldSeasonCommand, CommandResult>);
            services.AddTransient<ChangeWorldTimeCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldTimeCommandHandler>() as IRequestHandler<ChangeWorldTimeCommand, CommandResult>);
            services.AddTransient<ChangeWorldWeatherCommandHandler>();
            requestRegistrar.Add(sp => sp.GetRequiredService<ChangeWorldWeatherCommandHandler>() as IRequestHandler<ChangeWorldWeatherCommand, CommandResult>);

            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerJoinedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerLeavedNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerUpdatedStateNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentGlobalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentLocalMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<MessageToPlayerNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentRawP3DPacketNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<ServerMessageNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerTriggeredEventNotification>>());
            notificationRegistrar.Add(sp => sp.GetRequiredService<IPlayerContainerReader>().GetAll().OfType<INotificationHandler<PlayerSentCommandNotification>>());


            services.AddBetterHostedServices();

            if (isOfficial)
            {
                services.AddHttpClient("P3D.API")
                    .ConfigureHttpClient((sp, client) =>
                    {
                        var backendOptions = sp.GetRequiredService<IOptions<P3DOptions>>().Value;
                        client.BaseAddress = new Uri(backendOptions.APIEndpointV1);
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                        client.Timeout = Timeout.InfiniteTimeSpan;
                    })
                    .GenerateCorrelationId()
                    .AddPolly();
            }

            services.AddOpenTelemetryMetrics(builder =>
            {
                var options = configuration.GetSection("Otlp").Get<OtlpOptions>();
                if (options.Enabled)
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));
                    builder.AddAspNetCoreInstrumentation();
                    builder.AddHttpClientInstrumentation();
                    builder.AddStatisticsInstrumentation();
                    builder.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(options.Host);
                    });
                }
            });
            services.AddOpenTelemetryTracing(builder =>
            {
                var options = configuration.GetSection("Otlp").Get<OtlpOptions>();
                if (options.Enabled)
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("P3D.Legacy.Server"));
                    builder.AddAspNetCoreInstrumentation();
                    builder.AddHttpClientInstrumentation();
                    builder.AddHostInstrumentation();
                    builder.AddApplicationInstrumentation();
                    builder.AddInfrastructureInstrumentation();
                    builder.AddGameCommandsInstrumentation();
                    builder.AddStatisticsInstrumentation();
                    builder.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(options.Host);
                    });
                }
            });

            services.AddSingleton<DefaultJsonSerializer>();

            services.AddSingleton<P3DPacketFactory>();

            services.AddScoped<P3DConnectionContextHandler>();

            services.AddScoped<ConnectionContextHandlerFactory>();
            services.AddScoped<P3DProtocol>();

            services.AddSingleton<IPlayerIdGenerator, DefaultPlayerIdGenerator>();

            services.AddTransient<IBanQueries, BanQueries>();
            services.AddTransient<IPlayerQueries, PlayerQueries>();
            services.AddTransient<IPermissionQueries, PermissionQueries>();

            if (isOfficial)
            {
                services.AddTransient<IPermissionRepository, P3DPermissionRepository>();
                services.AddTransient<IBanRepository, LiteDbBanRepository>(); // TODO
            }
            else
            {
                services.AddTransient<IPermissionRepository, LiteDbPermissionRepository>();
                services.AddTransient<IBanRepository, LiteDbBanRepository>();
            }

            if (validationEnabled)
            {
                services.AddTransient<IMonsterRepository, PokeAPIMonsterRepository>();
            }
            else
            {
                services.AddTransient<IMonsterRepository, NopMonsterRepository>();
            }

            services.AddSingleton<DefaultPlayerContainer>();
            services.AddTransient<IPlayerContainerWriter>(sp => sp.GetRequiredService<DefaultPlayerContainer>());
            services.AddTransient<IPlayerContainerReader>(sp => sp.GetRequiredService<DefaultPlayerContainer>());

            services.AddHostedServiceAsSingleton<WorldService>();

            return services;
        }

        public static IServiceCollection AddMediatRInternal(this IServiceCollection services, RequestRegistrar requestRegistrar, NotificationRegistrar notificationRegistrar)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            requestRegistrar.Register(services);
            notificationRegistrar.Register(services);

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}