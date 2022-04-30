using Microsoft.Extensions.DependencyInjection;

using System;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class UIServiceScopeFactory : IServiceScopeFactory, IServiceProvider, IDisposable
    {
        private sealed class ScopeWrapper : IServiceScope
        {
            public IServiceProvider ServiceProvider => _serviceScope.ServiceProvider;

            private readonly UIServiceScopeFactory _scopeFactory;
            private readonly IServiceScope _serviceScope;

            public ScopeWrapper(UIServiceScopeFactory scopeFactory, IServiceScope serviceScope)
            {
                _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
                _serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));
            }

            public void Dispose()
            {
                _scopeFactory._currentScope = null;
            }
        }


        private readonly IServiceScopeFactory _serviceScopeFactory;
        private IServiceScope? _currentScope;

        public UIServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        }

        public IServiceScope CreateScope() => _currentScope = new ScopeWrapper(this, _serviceScopeFactory.CreateScope());

        public object? GetService(Type serviceType) => _currentScope?.ServiceProvider.GetService(serviceType);

        public void Dispose()
        {
            _currentScope?.Dispose();
        }
    }
}