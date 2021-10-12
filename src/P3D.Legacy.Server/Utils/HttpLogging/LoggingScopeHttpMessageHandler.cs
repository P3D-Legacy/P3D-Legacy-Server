using CorrelationId.Abstractions;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Utils.HttpLogging
{
    /// <summary>
    /// Handles logging of the lifecycle for an HTTP request within a log scope.
    /// </summary>
    public class LoggingScopeHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly HttpClientFactoryOptions? _options;

        private static readonly Func<string, bool> ShouldNotRedactHeaderValue = _ => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingScopeHttpMessageHandler"/> class with a specified logger.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <param name="correlationContextAccessor"></param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
        public LoggingScopeHttpMessageHandler(ILogger logger, ICorrelationContextAccessor correlationContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingScopeHttpMessageHandler"/> class with a specified logger and options.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <param name="options">The <see cref="HttpClientFactoryOptions"/> used to configure the <see cref="LoggingScopeHttpMessageHandler"/> instance.</param>
        /// <param name="correlationContextAccessor"></param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
        public LoggingScopeHttpMessageHandler(ILogger logger, HttpClientFactoryOptions options, ICorrelationContextAccessor correlationContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        /// <remarks>Logs the request to and response from the sent <see cref="HttpRequestMessage"/>.</remarks>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var stopwatch = ValueStopwatch.StartNew();

            var shouldRedactHeaderValue = _options?.ShouldRedactHeaderValue ?? ShouldNotRedactHeaderValue;

            using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", _correlationContextAccessor.CorrelationContext?.CorrelationId);
            using (Log.BeginRequestPipelineScope(_logger, request))
            {
                Log.RequestPipelineStart(_logger, request, shouldRedactHeaderValue);
                var response = await base.SendAsync(request, cancellationToken);
                Log.RequestPipelineEnd(_logger, response, stopwatch.GetElapsedTime(), shouldRedactHeaderValue);

                return response;
            }
        }

        // Used in tests
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId PipelineStart = new(100, "RequestPipelineStart");
                public static readonly EventId PipelineEnd = new(101, "RequestPipelineEnd");

                public static readonly EventId RequestHeader = new(102, "RequestPipelineRequestHeader");
                public static readonly EventId ResponseHeader = new(103, "RequestPipelineResponseHeader");
            }

            private static readonly Func<ILogger, HttpMethod, Uri?, IDisposable> BeginRequestPipelineScopeDelegate = LoggerMessage.DefineScope<HttpMethod, Uri?>("HTTP {HttpMethod} {Uri}");

            private static readonly Action<ILogger, HttpMethod, Uri?, Exception?> RequestPipelineStartDelegate = LoggerMessage.Define<HttpMethod, Uri?>(
                LogLevel.Information,
                EventIds.PipelineStart,
                "Start processing HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, double, int, Exception?> RequestPipelineEndDelegate = LoggerMessage.Define<double, int>(
                LogLevel.Information,
                EventIds.PipelineEnd,
                "End processing HTTP request after {ElapsedMilliseconds}ms - {StatusCode}");

            public static IDisposable BeginRequestPipelineScope(ILogger logger, HttpRequestMessage request)
            {
                return BeginRequestPipelineScopeDelegate(logger, request.Method, request.RequestUri);
            }

            public static void RequestPipelineStart(ILogger logger, HttpRequestMessage request, Func<string, bool> shouldRedactHeaderValue)
            {
                RequestPipelineStartDelegate(logger, request.Method, request.RequestUri, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.RequestHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Request, request.Headers, request.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            public static void RequestPipelineEnd(ILogger logger, HttpResponseMessage response, TimeSpan duration, Func<string, bool> shouldRedactHeaderValue)
            {
                RequestPipelineEndDelegate(logger, duration.TotalMilliseconds, (int) response.StatusCode, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.ResponseHeader,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Response, response.Headers, response.Content.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }
        }
    }
}