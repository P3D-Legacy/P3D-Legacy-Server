using CorrelationId.Abstractions;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;

namespace P3D.Legacy.Server.Utils.HttpLogging
{
    internal sealed class LoggingHttpMessageHandlerBuilderFilter : IHttpMessageHandlerBuilderFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IOptionsMonitor<HttpClientFactoryOptions> _optionsMonitor;

        public LoggingHttpMessageHandlerBuilderFilter(ILoggerFactory loggerFactory, IOptionsMonitor<HttpClientFactoryOptions> optionsMonitor, ICorrelationContextAccessor correlationContextAccessor)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
            _optionsMonitor = optionsMonitor ?? throw new ArgumentNullException(nameof(optionsMonitor));
        }

        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return builder =>
            {
                // Run other configuration first, we want to decorate.
                next(builder);

                var loggerName = !string.IsNullOrEmpty(builder.Name) ? builder.Name : "Default";

                // We want all of our logging message to show up as-if they are coming from HttpClient,
                // but also to include the name of the client for more fine-grained control.
                var outerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.LogicalHandler");
                var innerLogger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{loggerName}.ClientHandler");

                var options = _optionsMonitor.Get(builder.Name);

                // The 'scope' handler goes first so it can surround everything.
                builder.AdditionalHandlers.Insert(0, new LoggingScopeHttpMessageHandler(outerLogger, options, _correlationContextAccessor));

                // We want this handler to be last so we can log details about the request after
                // service discovery and security happen.
                builder.AdditionalHandlers.Add(new LoggingHttpMessageHandler(innerLogger, options, _correlationContextAccessor));
            };
        }
    }
}