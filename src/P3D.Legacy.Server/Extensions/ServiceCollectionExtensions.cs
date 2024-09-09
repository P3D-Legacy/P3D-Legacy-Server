using BetterHostedServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Behaviours;
using P3D.Legacy.Server.CQERS.Behaviours.Command;
using P3D.Legacy.Server.CQERS.Behaviours.Query;
using P3D.Legacy.Server.Options;

using System;
using System.Net.Http.Headers;
using System.Threading;

namespace P3D.Legacy.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHost(this IServiceCollection services)
    {
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandValidationBehaviour<>));
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandTracingBehaviour<>));

        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryValidationBehaviour<,>));
        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryTracingBehaviour<,>));

        services.AddBetterHostedServices();

        services.AddHttpClient("P3D.API")
            .ConfigureHttpClient(static (sp, client) =>
            {
                var backendOptionsSnapshot = sp.GetRequiredService<IOptions<P3DSiteOptions>>();
                var backendOptions = backendOptionsSnapshot.Value;

                client.BaseAddress = new Uri("https+http://officialsite");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", backendOptions.APIToken);
                client.Timeout = Timeout.InfiniteTimeSpan;
            })
            .AddPolly();

        return services;
    }
}