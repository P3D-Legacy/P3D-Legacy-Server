using CorrelationId.Abstractions;

using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.HttpLogging
{
    /// <summary>
    /// Handles logging of the lifecycle for an HTTP request.
    /// </summary>
    public class LoggingHttpMessageHandler : DelegatingHandler
    {
        private readonly ILogger _logger;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly HttpClientFactoryOptions? _options;

        private static readonly Func<string, bool> ShouldNotRedactHeaderValue = _ => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHttpMessageHandler"/> class with a specified logger.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <param name="correlationContextAccessor"></param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/>.</exception>
        public LoggingHttpMessageHandler(ILogger logger, ICorrelationContextAccessor correlationContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHttpMessageHandler"/> class with a specified logger and options.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to log to.</param>
        /// <param name="options">The <see cref="HttpClientFactoryOptions"/> used to configure the <see cref="LoggingHttpMessageHandler"/> instance.</param>
        /// <param name="correlationContextAccessor"></param>
        /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
        public LoggingHttpMessageHandler(ILogger logger, HttpClientFactoryOptions options, ICorrelationContextAccessor correlationContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        /// <remarks>Loggs the request to and response from the sent <see cref="HttpRequestMessage"/>.</remarks>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var shouldRedactHeaderValue = _options?.ShouldRedactHeaderValue ?? ShouldNotRedactHeaderValue;

            using var scope = _logger.BeginScope("CorrelationId: {CorrelationId}", _correlationContextAccessor.CorrelationContext?.CorrelationId);
            Log.RequestStart(_logger, request, shouldRedactHeaderValue);
            var stopwatch = ValueStopwatch.StartNew();
            var response = await base.SendAsync(request, cancellationToken);
            Log.RequestEnd(_logger, response, stopwatch.GetElapsedTime(), shouldRedactHeaderValue);

            return response;
        }

        // Used in tests.
        internal static class Log
        {
            public static class EventIds
            {
                public static readonly EventId RequestStartEventId = new(100, "RequestStart");
                public static readonly EventId RequestEndEventId = new(101, "RequestEnd");

                public static readonly EventId RequestHeaderEventId = new(102, "RequestHeader");
                public static readonly EventId ResponseHeaderEventId = new(103, "ResponseHeader");
            }

            private static readonly Action<ILogger, HttpMethod, Uri?, Exception?> RequestStartDelegate = LoggerMessage.Define<HttpMethod, Uri?>(
                LogLevel.Information,
                EventIds.RequestStartEventId,
                "Sending HTTP request {HttpMethod} {Uri}");

            private static readonly Action<ILogger, double, int, Exception?> RequestEndDelegate = LoggerMessage.Define<double, int>(
                LogLevel.Information,
                EventIds.RequestEndEventId,
                "Received HTTP response headers after {ElapsedMilliseconds}ms - {StatusCode}");

            public static void RequestStart(ILogger logger, HttpRequestMessage request, Func<string, bool> shouldRedactHeaderValue)
            {
                RequestStartDelegate(logger, request.Method, request.RequestUri, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.RequestHeaderEventId,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Request, request.Headers, request.Content?.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }

            public static void RequestEnd(ILogger logger, HttpResponseMessage response, TimeSpan duration, Func<string, bool> shouldRedactHeaderValue)
            {
                RequestEndDelegate(logger, duration.TotalMilliseconds, (int) response.StatusCode, null);

                if (logger.IsEnabled(LogLevel.Trace))
                {
                    logger.Log(
                        LogLevel.Trace,
                        EventIds.ResponseHeaderEventId,
                        new HttpHeadersLogValue(HttpHeadersLogValue.Kind.Response, response.Headers, response.Content.Headers, shouldRedactHeaderValue),
                        null,
                        (state, ex) => state.ToString());
                }
            }
        }
    }
}