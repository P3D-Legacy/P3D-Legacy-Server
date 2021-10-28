using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    public class RequestRegistrar : IEnumerable
    {
        private class RequestServiceFactory<TRequest, TResponse> : BaseServiceFactory where TRequest : IRequest<TResponse>
        {
            private Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> _command = default!;

            public void Register(Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> func) => _command = func;

            public override IRequestHandler<TRequest, TResponse> ServiceFactory(IServiceProvider sp) => _command(sp);
        }


        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();

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
                container.Register(sp => factory(sp));
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