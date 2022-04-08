using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.GUI.Services;

using Serilog;
using Serilog.Configuration;

using System;

namespace P3D.Legacy.Server.GUI.Extensions
{
    public static class LogsTabViewExtensions
    {
        public static LoggerConfiguration UI(this LoggerSinkConfiguration configuration, IServiceProvider serviceProvider) =>
            configuration.Sink(serviceProvider.GetRequiredService<UILogEventSink>());
    }
}