using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.GUI.Views;

using System;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class UILogger : ILogger
    {
        private sealed class NopDisposable : IDisposable
        {
            public void Dispose() { }
        }

        private readonly string _categoryName;
        private readonly UIServiceScopeFactory _uiServiceScopeFactory;

        public UILogger(string categoryName, UIServiceScopeFactory uiServiceScopeFactory)
        {
            _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            _uiServiceScopeFactory = uiServiceScopeFactory ?? throw new ArgumentNullException(nameof(uiServiceScopeFactory));
        }


        public IDisposable BeginScope<TState>(TState state) where TState : notnull => new NopDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _uiServiceScopeFactory.GetService<LogsTabView>()?.Log(_categoryName, logLevel, eventId, state, exception, formatter);
        }
    }
}