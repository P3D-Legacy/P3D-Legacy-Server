using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.GUI.Views;

using Serilog.Core;
using Serilog.Events;

using System;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class UILogEventSink : ILogEventSink
    {
        private readonly UIServiceScopeFactory _uiServiceScopeFactory;

        public UILogEventSink(UIServiceScopeFactory uiServiceScopeFactory)
        {
            _uiServiceScopeFactory = uiServiceScopeFactory ?? throw new ArgumentNullException(nameof(uiServiceScopeFactory));
        }

        public void Emit(LogEvent logEvent) => _uiServiceScopeFactory.GetService<LogsTabView>()?.Log(logEvent);
    }
}