using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.Utils
{
    internal class RequestRegistrar : IEnumerable
    {
        private class RequestServiceFactory<TRequest> : BaseServiceFactory where TRequest : IRequest
        {
            private readonly List<Func<IServiceProvider, IEnumerable<IRequestHandler<TRequest>>>> _notifications = new();

            public void Register(Func<IServiceProvider, IEnumerable<IRequestHandler<TRequest>>> func) => _notifications.Add(func);

            public override IEnumerable<IRequestHandler<TRequest>> ServiceFactory(IServiceProvider sp) => _notifications.SelectMany(func => func(sp));
        }

        private readonly Dictionary<(Type, ServiceLifetime), BaseServiceFactory> _containers = new();

        public void Add<TRequest>(Func<IServiceProvider, IRequestHandler<TRequest>> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TRequest : IRequest
        {
            var key = (typeof(IEnumerable<IRequestHandler<TRequest>>), lifetime);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new RequestServiceFactory<TRequest>();
                _containers.Add(key, cont);
            }

            if (cont is RequestServiceFactory<TRequest> container)
            {
                container.Register(sp => new[] { factory(sp) });
            }
        }

        public void Add<TRequest>(Func<IServiceProvider, IEnumerable<IRequestHandler<TRequest>>> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TRequest : IRequest
        {
            var key = (typeof(IEnumerable<IRequestHandler<TRequest>>), lifetime);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new RequestServiceFactory<TRequest>();
                _containers.Add(key, cont);
            }

            if (cont is RequestServiceFactory<TRequest> container)
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