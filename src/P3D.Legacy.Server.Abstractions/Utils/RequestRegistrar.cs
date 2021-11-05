using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    public sealed class RequestRegistrar : IEnumerable, IDisposable
    {
        private class RequestServiceFactory<TRequest, TResponse> : BaseServiceFactory where TRequest : IRequest<TResponse>
        {
            private Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> _command = default!;

            public void Register(Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> func) => _command = func;

            public override IRequestHandler<TRequest, TResponse> ServiceFactory(IServiceProvider sp) => _command(sp);
        }

        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();

        public RequestRegistrar(IServiceCollection services) => _services = services;

        public void Add<TRequest, TResponse>(Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> factory) where TRequest : IRequest<TResponse>
        {
            var key = typeof(IRequestHandler<TRequest, TResponse>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new RequestServiceFactory<TRequest, TResponse>();
                _containers.Add(key, cont);
            }

            if (cont is RequestServiceFactory<TRequest, TResponse> container)
            {
                container.Register(factory);
            }
        }

        public void AddWithRegistration<TRequest, TResponse, TService>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TRequest : IRequest<TResponse> where TService : IRequestHandler<TRequest, TResponse>
        {
            var key = typeof(IRequestHandler<TRequest, TResponse>);

            if (!_containers.TryGetValue(key, out var cont))
            {
                cont = new RequestServiceFactory<TRequest, TResponse>();
                _containers.Add(key, cont);
            }

            if (cont is RequestServiceFactory<TRequest, TResponse> container)
            {
                _services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), lifetime));
                container.Register(sp => sp.GetRequiredService<TService>() as IRequestHandler<TRequest, TResponse>);
            }
        }

        public void AddWithRegistration<TRequest, TService>(ServiceLifetime lifetime = ServiceLifetime.Transient) where TRequest : IRequest<Unit> where TService : IRequestHandler<TRequest, Unit>
        {
            AddWithRegistration<TRequest, Unit, TService>(lifetime);
        }

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();

        public void Dispose()
        {
            foreach (var (type, container) in _containers)
                _services.Add(ServiceDescriptor.Describe(type, sp => container.ServiceFactory(sp), ServiceLifetime.Transient));
        }
    }
}