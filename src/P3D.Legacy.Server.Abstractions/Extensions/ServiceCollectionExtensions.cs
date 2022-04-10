using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private class NotificationHandlerWrapper<TNotificationHandler, TNotification> : INotificationHandler<TNotification>
            where TNotification : INotification
            where TNotificationHandler : INotificationHandler<TNotification>
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

        private class NotificationHandlerEnumerableWrapper<TNotificationHandler, TNotification> : INotificationHandler<TNotification>
            where TNotification : INotification
            where TNotificationHandler : INotificationHandler<TNotification>
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

        private class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> _factory;

            public RequestHandlerWrapper(IServiceProvider serviceProvider, Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> factory)
            {
                _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public Task<TResponse> Handle(TRequest request, CancellationToken ct)
            {
                if (_factory(_serviceProvider) is { } requestHandler)
                    return requestHandler.Handle(request, ct);
                else
                    return Task.FromResult<TResponse>(default!);
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
            where TNotificationHandler : INotificationHandler<TNotification>
        {
            services.Add(ServiceDescriptor.Transient<INotificationHandler<TNotification>>(sp => new NotificationHandlerWrapper<TNotificationHandler, TNotification>(sp, factory)));

            return services;
        }
        public static IServiceCollection AddNotificationEnumerable<TNotificationHandler, TNotification>(this IServiceCollection services, Func<IServiceProvider, IEnumerable<TNotificationHandler>> factory)
            where TNotification : INotification
            where TNotificationHandler : INotificationHandler<TNotification>
        {
            services.Add(ServiceDescriptor.Transient<INotificationHandler<TNotification>>(sp => new NotificationHandlerEnumerableWrapper<TNotificationHandler, TNotification>(sp, factory)));

            return services;
        }

        public static IServiceCollection AddRequest<TRequest, TResponse>(this IServiceCollection services, Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> factory) where TRequest : IRequest<TResponse>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TRequest, TResponse>>(sp => new RequestHandlerWrapper<TRequest, TResponse>(sp, factory)));

            return services;
        }

        public static IServiceCollection AddRequest<TRequest, TResponse, TService>(this IServiceCollection services) where TRequest : IRequest<TResponse> where TService : class, IRequestHandler<TRequest, TResponse>
        {
            services.Add(ServiceDescriptor.Transient<IRequestHandler<TRequest, TResponse>, TService>());

            return services;
        }

        public static IServiceCollection AddRequest<TRequest, TService>(this IServiceCollection services) where TRequest : IRequest<Unit> where TService : class, IRequestHandler<TRequest, Unit>
        {
            services.AddRequest<TRequest, Unit, TService>();

            return services;
        }
    }
}
