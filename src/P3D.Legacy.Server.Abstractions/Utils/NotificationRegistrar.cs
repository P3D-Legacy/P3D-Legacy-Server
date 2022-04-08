using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    public sealed class NotificationRegistrar : IEnumerable, IDisposable
    {
        private class NotificationServiceFactory<TNotification> : BaseServiceFactory where TNotification : INotification
        {
            private readonly List<Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>?>>> _notifications = new();

            public void Register(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>?>> func) => _notifications.Add(func);

            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
            private void RegisterInternal(Func<IServiceProvider, IEnumerable> func)
            {
                IEnumerable<INotificationHandler<TNotification>> Convert(IServiceProvider sp, Func<IServiceProvider, IEnumerable> internalFunc)
                {
                    foreach (var obj in internalFunc(sp))
                    {
                        yield return (obj as INotificationHandler<TNotification>)!;
                    }
                }

                _notifications.Add(sp => Convert(sp, func));
            }

            public override IEnumerable<INotificationHandler<TNotification>> ServiceFactory(IServiceProvider sp) => _notifications.SelectMany(func => func(sp)).OfType<INotificationHandler<TNotification>>();
        }

        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();

        public NotificationRegistrar(IServiceCollection services) => _services = services;

        public void Add<TNotification>(Func<IServiceProvider, INotificationHandler<TNotification>?> factory) where TNotification : INotification
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

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

        public void Add<TNotification>(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> factory) where TNotification : INotification
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

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

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();

        public void Dispose()
        {
            foreach (var (type, container) in _containers)
                _services.Add(ServiceDescriptor.Describe(type, sp => container.ServiceFactory(sp), ServiceLifetime.Transient));
        }
    }
}