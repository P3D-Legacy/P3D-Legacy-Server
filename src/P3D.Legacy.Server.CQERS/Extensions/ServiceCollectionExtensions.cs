using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Behaviours.Command;
using P3D.Legacy.Server.CQERS.Behaviours.Query;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Services;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Events;
using P3D.Legacy.Server.Domain.Queries;

namespace P3D.Legacy.Server.CQERS.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandPostProcessorBehavior<>));
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandPreProcessorBehavior<>));
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandPerformanceBehaviour<>));
        services.AddTransient(typeof(ICommandBehavior<>), typeof(CommandUnhandledExceptionBehaviour<>));
        services.AddTransient(typeof(ICommandPreProcessor<>), typeof(CommandLoggingBehaviour<>));

        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryPostProcessorBehavior<,>));
        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryPreProcessorBehavior<,>));
        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryPerformanceBehaviour<,>));
        services.AddTransient(typeof(IQueryBehavior<,>), typeof(QueryUnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IQueryPreProcessor<>), typeof(QueryLoggingBehaviour<>));

        services.AddSingleton(typeof(CommandDispatcherHelper<>));
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        services.AddSingleton(typeof(QueryDispatcherHelper<,>));
        services.AddSingleton<IQueryDispatcher, QueryDispatcher>();

        services.AddSingleton<IEventDispatcher, EventDispatcher>();

        services.AddSingleton<ReceiveContextFactory>();

        return services;
    }
}