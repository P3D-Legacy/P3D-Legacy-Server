using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Utils
{
    internal class NotificationRegistrar : IEnumerable
    {
        private class NotificationServiceFactory<TNotification> : BaseServiceFactory where TNotification : INotification
        {
            private readonly List<Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>>> _notifications = new();

            public void Register(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> func) => _notifications.Add(func);

            public override IEnumerable<INotificationHandler<TNotification>> ServiceFactory(IServiceProvider sp) => _notifications.SelectMany(func => func(sp));
        }

        private readonly Dictionary<(Type, ServiceLifetime), BaseServiceFactory> _containers = new();

        public void Add<TNotification>(Func<IServiceProvider, INotificationHandler<TNotification>> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TNotification : INotification
        {
            var key = (typeof(IEnumerable<INotificationHandler<TNotification>>), lifetime);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(sp => new[] { factory(sp) });
            }
        }

        public void Add<TNotification>(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TNotification : INotification
        {
            var key = (typeof(IEnumerable<INotificationHandler<TNotification>>), lifetime);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(factory);
            }
        }

        public IServiceCollection Register(IServiceCollection services)
        {
            foreach (var ((type, lifetime), container) in _containers)
                services.Add(ServiceDescriptor.Describe(type, sp => container.ServiceFactory(sp), lifetime));

            return services;
        }

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();
    }
}