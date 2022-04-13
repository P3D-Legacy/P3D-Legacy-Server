using BetterHostedServices;

using MediatR;
using MediatR.Pipeline;
using MediatR.Registration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Services;
using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.Options;

using System;
using System.Net.Http.Headers;
using System.Threading;

namespace P3D.Legacy.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHost(this IServiceCollection services)
        {
            services.AddBetterHostedServices();

            services.AddHttpClient("P3D.API")
                .ConfigureHttpClient((sp, client) =>
                {
                    var backendOptionsSnapshot = sp.GetRequiredService<IOptions<P3DSiteOptions>>();
                    var backendOptions = backendOptionsSnapshot.Value;

                    client.BaseAddress = new Uri(backendOptions.APIEndpointV1);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                    client.Timeout = Timeout.InfiniteTimeSpan;
                })
                .AddPolly();

            return services;
        }

        public static IServiceCollection AddMediatRInternal(this IServiceCollection services)
        {
            ServiceRegistrar.AddRequiredServices(services, new MediatRServiceConfiguration().AsTransient());

            services.AddTransient<NotificationPublisher>();

            services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TracingBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

            return services;
        }
    }
}