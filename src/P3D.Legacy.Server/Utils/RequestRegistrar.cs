using MediatR;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Utils
{
    internal class RequestRegistrar : IEnumerable
    {
        private class RequestServiceFactory<TRequest, TResponse> : BaseServiceFactory where TRequest : IRequest<TResponse>
        {
            private Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> _command;

            public void Register(Func<IServiceProvider, IRequestHandler<TRequest, TResponse>> func) => _command = func;

            public override IRequestHandler<TRequest, TResponse> ServiceFactory(IServiceProvider sp) => _command(sp);
        }


        private readonly Dictionary<Type, BaseServiceFactory> _containers = new();
        private readonly Dictionary<Type, Type> _direct = new();

        public void Add(Type @base, Type impl)
        {
            if (!@base.IsInterface || (@base.GetGenericTypeDefinition() != typeof(IRequestHandler<>) && @base.GetGenericTypeDefinition() != typeof(IRequestHandler<,>)))
                throw new Exception();

            if (!@base.IsAssignableFrom(impl))
                throw new Exception();

            if (@base.GenericTypeArguments.Length != 2)
                @base = typeof(IRequestHandler<,>).MakeGenericType(@base.GenericTypeArguments[0], typeof(Unit));

            _direct[@base] = impl;
        }

        public void Add<TImpl, TRequest, TResponse>() where TRequest : IRequest<TResponse> where TImpl : IRequestHandler<TRequest, TResponse>
        {
            var key = typeof(IRequestHandler<TRequest, TResponse>);
            _direct[key] = typeof(TImpl);
        }

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

            foreach (var (@base, impl) in _direct)
                services.Add(ServiceDescriptor.Describe(@base, impl, ServiceLifetime.Transient));

            return services;
        }

        public IEnumerator GetEnumerator() => _containers.GetEnumerator();
    }
}