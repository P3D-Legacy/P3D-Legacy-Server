using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3D.Legacy.Server.Utils
{
    internal class NotificationRegistrar : IEnumerable
    {
        private class NotificationServiceFactory<TNotification> : BaseServiceFactory where TNotification : INotification
        {
            private readonly List<Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>>> _notifications = new();

            public void Register(Func<IServiceProvider, IEnumerable<INotificationHandler<TNotification>>> func) => _notifications.Add(func);

            private void RegisterInternal(Func<IServiceProvider, IEnumerable> func)
            {
                IEnumerable<INotificationHandler<TNotification>> Convert(IServiceProvider sp, Func<IServiceProvider, IEnumerable> func_)
                {
                    foreach (var obj in func(sp))
                    {
                        yield return obj as INotificationHandler<TNotification>;
                    }
                }

                _notifications.Add(sp => Convert(sp, func));
            }

            public override IEnumerable<INotificationHandler<TNotification>> ServiceFactory(IServiceProvider sp) => _notifications.SelectMany(func => func(sp));
        }

        private static readonly MethodInfo GenericAddMethod = typeof(NotificationRegistrar).GetMethod("Add", Type.EmptyTypes)!;


        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();

        public void Add(Type @base, Type impl)
        {
            if (!ReflectionUtils.IsAssignableToGenericType(@base, typeof(INotificationHandler<>)))
                throw new Exception();

            if (!ReflectionUtils.IsAssignableToGenericType(impl, typeof(INotificationHandler<>)))
                throw new Exception();

            if (!@base.IsAssignableFrom(impl))
                throw new Exception();

            if (@base.GenericTypeArguments.Length != 1)
                throw new Exception();

            var notificationType = @base.GenericTypeArguments[0];
            if (notificationType is null)
                throw new Exception();

            var method = GenericAddMethod.MakeGenericMethod(impl, notificationType);
            method.Invoke(this, Array.Empty<object>());
        }

        public void Add<TImpl, TNotification>() where TNotification : INotification where TImpl : INotificationHandler<TNotification>
        {
            var key = typeof(IEnumerable<INotificationHandler<TNotification>>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new NotificationServiceFactory<TNotification>();
                _containers.Add(key, cont);
            }

            if (cont is NotificationServiceFactory<TNotification> container)
            {
                container.Register(sp => new[] { sp.GetRequiredService<TImpl>() as INotificationHandler<TNotification> });
            }
        }

        public void Add<TNotification>(Func<IServiceProvider, INotificationHandler<TNotification>> factory) where TNotification : INotification
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

        public IServiceCollection Register(IServiceCollection services)
        {
            foreach (var (type, container) in _containers)
                services.Add(ServiceDescriptor.Describe(type, sp => container.ServiceFactory(sp), ServiceLifetime.Transient));

            return services;
        }

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();
    }
}