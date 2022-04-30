using Microsoft.Extensions.Logging;

using System;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class UILoggerProvider : ILoggerProvider
    {
        private readonly UIServiceScopeFactory _uiServiceScopeFactory;

        public UILoggerProvider(UIServiceScopeFactory uiServiceScopeFactory)
        {
            _uiServiceScopeFactory = uiServiceScopeFactory ?? throw new ArgumentNullException(nameof(uiServiceScopeFactory));
        }

        public ILogger CreateLogger(string categoryName) => new UILogger(categoryName, _uiServiceScopeFactory);

        public void Dispose() { }
    }
}