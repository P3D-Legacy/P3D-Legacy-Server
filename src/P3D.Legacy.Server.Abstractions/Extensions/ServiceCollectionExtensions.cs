using MediatR;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Queries;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using INotification = P3D.Legacy.Server.Abstractions.Notifications.INotification;

namespace P3D.Legacy.Server.Abstractions.Extensions
{
    public class DynamicConfigurationProviderManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDynamicConfigurationProvider[] _configurationProviders = Array.Empty<IDynamicConfigurationProvider>();

        public DynamicConfigurationProviderManager(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if (configuration is IConfigurationRoot root)
            {
                _configurationProviders = root.Providers.OfType<IDynamicConfigurationProvider>().ToArray();
            }
        }

        public IEnumerable<Type> GetRegisteredOptionTypes() => _configurationProviders.Select(x => x.OptionsType);

        public IDynamicConfigurationProvider? GetProvider(Type optionsType) => _configurationProviders.FirstOrDefault(x => x.OptionsType == optionsType);
        public IDynamicConfigurationProvider? GetProvider<TOptions>() => GetProvider(typeof(TOptions));
        public object? GetOptions(Type type)
        {
            var openMethod = typeof(DynamicConfigurationProviderManager).GetMethod("GetOptionsInternal", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var method = openMethod?.MakeGenericMethod(type);
            return method?.Invoke(this, Array.Empty<object>());
        }
        public TOptions? GetOptionsInternal<TOptions>() where TOptions : class => _serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
    }

    public interface IDynamicConfigurationProvider
    {
        public Type OptionsType { get; }

        public IEnumerable<PropertyInfo> AvailableProperties { get; }

        public bool SetProperty(PropertyInfo propertyInfo, string value);
    }

    public class MemoryConfigurationProvider<TOptions> : ConfigurationProvider, IConfigurationSource, IDynamicConfigurationProvider
    {
        public Type OptionsType => typeof(TOptions);
        public IEnumerable<PropertyInfo> AvailableProperties => _keys;

        private readonly string _basePath;
        private readonly PropertyInfo[] _keys;
        public MemoryConfigurationProvider(IConfigurationSection section)
        {
            _basePath = section.Path;
            _keys = typeof(TOptions).GetProperties().Where(p => p.CanRead && p.CanWrite).ToArray();
        }

        public bool SetProperty(PropertyInfo propertyInfo, string value)
        {
            Set($"{_basePath}:{propertyInfo.Name}", value);
            OnReload();
            return true;
        }

        public override void Load() { }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;
    }

    public static class ServiceCollectionExtensions
    {
        private class NotificationHandlerWrapper<TNotificationHandler, TNotification> : Notifications.INotificationHandler<TNotification>
            where TNotification : INotification
            where TNotificationHandler : Notifications.INotificationHandler<TNotification>
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Func<IServiceProvider, TNotificationHandler?> _factory;

            public NotificationHandlerWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, TNotificationHandler?> factory)
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public Task Handle(TNotification notification, CancellationToken ct)
            {
                if (_factory(_serviceProvider) is { } notificationHandler)
                    return notificationHandler.Handle(notification, ct);
                else
                    return Task.CompletedTask;
            }
        }
        private class NotificationHandlerEnumerableWrapper<TNotificationHandler, TNotification> : Notifications.INotificationHandler<TNotification>
            where TNotification : INotification
            where TNotificationHandler : Notifications.INotificationHandler<TNotification>
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Func<IServiceProvider, IEnumerable<TNotificationHandler>> _factory;

            public NotificationHandlerEnumerableWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, IEnumerable<TNotificationHandler>> factory)
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public async Task Handle(TNotification notification, CancellationToken ct)
            {
                foreach (var notificationHandler in _factory(_serviceProvider))
                {
                    await notificationHandler.Handle(notification, ct);
                }
            }
        }
        private class CommandHandlerWrapper<TCommand> : ICommandHandler<TCommand>
            where TCommand : ICommand
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Func<IServiceProvider, ICommandHandler<TCommand>> _factory;

            public CommandHandlerWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, ICommandHandler<TCommand>> factory)
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public Task<CommandResult> Handle(TCommand request, CancellationToken ct)
            {
                if (_factory(_serviceProvider) is { } requestHandler)
                    return requestHandler.Handle(request, ct);
                else
                    return Task.FromResult(CommandResult.Failure);
            }
        }
        private class QueryHandlerWrapper<TQuery, TQueryResult> : IQueryHandler<TQuery, TQueryResult>
            where TQuery : IQuery<TQueryResult>
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Func<IServiceProvider, IQueryHandler<TQuery, TQueryResult>> _factory;

            public QueryHandlerWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, IQueryHandler<TQuery, TQueryResult>> factory)
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public Task<TQueryResult> Handle(TQuery request, CancellationToken ct)
            {
                if (_factory(_serviceProvider) is { } requestHandler)
                    return requestHandler.Handle(request, ct);
                else
                    return Task.FromResult<TQueryResult>(default!);
            }
        }

        public static IServiceCollection AddNotifications<TNotificationHandler>(this IServiceCollection services, Func<IServiceProvider, TNotificationHandler> factory)
        {
            var @typeof = typeof(TNotificationHandler);
            var @typeofNotificationInterfaces = @typeof.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>)).ToArray();
            foreach (var notificationInterface in @typeofNotificationInterfaces)
            {
                var genericType = notificationInterface.GenericTypeArguments[0];
                var openMethod = typeof(ServiceCollectionExtensions).GetMethod("AddNotification", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var method = openMethod?.MakeGenericMethod(@typeof, genericType);
                method?.Invoke(null, new object?[] { services, factory });
            }

            return services;
        }
        public static IServiceCollection AddNotificationsEnumerable<TNotificationHandler>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<TNotificationHandler>> factory)
        {
            var @typeof = typeof(TNotificationHandler);
            var @typeofNotificationInterfaces = @typeof.GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(INotificationHandler<>));
            foreach (var notificationInterface in @typeofNotificationInterfaces)
            {
                var genericType = notificationInterface.GenericTypeArguments[0];
                var openMethod = typeof(ServiceCollectionExtensions).GetMethod("AddNotificationEnumerable", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var method = openMethod?.MakeGenericMethod(@typeof, genericType);
                method?.Invoke(null, new object?[] { services, factory });
            }

            return services;
        }
        public static IServiceCollection AddNotification<TNotificationHandler, TNotification>(this IServiceCollection services, Func<IServiceProvider, TNotificationHandler?> factory)
            where TNotification : INotification
            where TNotificationHandler : Notifications.INotificationHandler<TNotification>
        {
            services.Add(ServiceDescriptor.Transient<INotificationHandler<TNotification>>(sp => new NotificationHandlerWrapper<TNotificationHandler, TNotification>(sp, factory)));

            return services;
        }
        public static IServiceCollection AddNotificationEnumerable<TNotificationHandler, TNotification>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<TNotificationHandler>> factory)
            where TNotification : INotification
            where TNotificationHandler : Notifications.INotificationHandler<TNotification>
        {
            services.Add(ServiceDescriptor.Transient<INotificationHandler<TNotification>>(sp => new NotificationHandlerEnumerableWrapper<TNotificationHandler, TNotification>(sp, factory)));

            return services;
        }

        public static IServiceCollection AddCommand<TCommand>(this IServiceCollection services, Func<IServiceProvider, ICommandHandler<TCommand>> factory)
            where TCommand : ICommand
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TCommand, CommandResult>>(sp => new CommandHandlerWrapper<TCommand>(sp, factory)));

            return services;
        }
        public static IServiceCollection AddCommand<TCommand, TService>(this IServiceCollection services)
            where TCommand : ICommand
            where TService : class, ICommandHandler<TCommand>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TCommand, CommandResult>, TService>());

            return services;
        }
        public static IServiceCollection AddCommand<TCommand, TService>(this IServiceCollection services, Func<IServiceProvider, TService> factory)
            where TCommand : ICommand
            where TService : class, ICommandHandler<TCommand>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TCommand, CommandResult>, TService>(factory));

            return services;
        }
        public static IServiceCollection AddCommandHandler<TCommandHandler>(this IServiceCollection services)
            where TCommandHandler : class, ICommandHandler
        {
            foreach (var requestHandlerType in typeof(TCommandHandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                var baseType = typeof(IRequestHandler<,>).MakeGenericType(requestHandlerType.GenericTypeArguments[0], typeof(CommandResult));
                var implType = typeof(TCommandHandler);

                services.Add(ServiceDescriptor.Transient(baseType, implType));
            }

            return services;
        }
        public static IServiceCollection AddCommandHandler<TCommandHandler>(this IServiceCollection services, Func<IServiceProvider, TCommandHandler> factory)
            where TCommandHandler : class, ICommandHandler
        {
            foreach (var requestHandlerType in typeof(TCommandHandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                var baseType = typeof(IRequestHandler<,>).MakeGenericType(requestHandlerType.GenericTypeArguments[0], typeof(CommandResult));

                services.Add(ServiceDescriptor.Transient(baseType, factory));
            }

            return services;
        }

        public static IServiceCollection AddQuery<TQuery, TQueryResult>(this IServiceCollection services, Func<IServiceProvider, IQueryHandler<TQuery, TQueryResult>> factory)
            where TQuery : IQuery<TQueryResult>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TQuery, TQueryResult>>(sp => new QueryHandlerWrapper<TQuery, TQueryResult>(sp, factory)));

            return services;
        }
        public static IServiceCollection AddQuery<TQuery, TQueryResult, TService>(this IServiceCollection services)
            where TQuery : IQuery<TQueryResult>
            where TService : class, IQueryHandler<TQuery, TQueryResult>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TQuery, TQueryResult>, TService>());

            return services;
        }
        public static IServiceCollection AddQueryHandler<TQueryHandler>(this IServiceCollection services)
            where TQueryHandler : class, IQueryHandler
        {
            foreach (var requestHandlerType in typeof(TQueryHandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            {
                var baseType = typeof(IRequestHandler<,>).MakeGenericType(requestHandlerType.GenericTypeArguments[0], requestHandlerType.GenericTypeArguments[1]);
                var implType = typeof(TQueryHandler);

                services.Add(ServiceDescriptor.Transient(baseType, implType));
            }

            return services;
        }
        public static IServiceCollection AddQueryHandler<TQueryHandler>(this IServiceCollection services, Func<IServiceProvider, TQueryHandler> factory)
            where TQueryHandler : class, IQueryHandler
        {
            foreach (var requestHandlerType in typeof(TQueryHandler).GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            {
                var baseType = typeof(IRequestHandler<,>).MakeGenericType(requestHandlerType.GenericTypeArguments[0], requestHandlerType.GenericTypeArguments[1]);

                services.Add(ServiceDescriptor.Transient(baseType, factory));
            }

            return services;
        }
    }
}