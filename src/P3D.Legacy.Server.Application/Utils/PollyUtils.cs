using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Utils;

public sealed partial class PollyUtils
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Exception during HTTP connection. HttpResult {@HttpResult}. Retry count {RetryCount}. Waiting {Time}...")]
    private static partial void Exception(ILogger logger, HttpResponseMessage httpResult, int retryCount, TimeSpan time, Exception exception);

    private static TimeSpan GetServerWaitDuration(DelegateResult<HttpResponseMessage> response)
    {
        if (response.Result?.Headers.RetryAfter is not { } retryAfter)
            return TimeSpan.Zero;

        return retryAfter.Date is not null ? retryAfter.Date.Value - DateTime.UtcNow : retryAfter.Delta.GetValueOrDefault(TimeSpan.Zero);
    }

    public static IAsyncPolicy<HttpResponseMessage> PolicySelector(IServiceProvider sp, HttpRequestMessage _)
    {
        var logger = sp.GetRequiredService<ILogger<PollyUtils>>();

        return Policy
            .Handle<Exception>()
            .Or<HttpRequestException>(static e => e.StatusCode != HttpStatusCode.Unauthorized)
            .OrTransientHttpStatusCode()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: static (_, result, _) =>
                {
                    var clientWaitDuration = TimeSpan.FromSeconds(2);
                    var serverWaitDuration = GetServerWaitDuration(result);
                    var waitDuration = Math.Max(clientWaitDuration.TotalMilliseconds, serverWaitDuration.TotalMilliseconds);
                    return TimeSpan.FromMilliseconds(waitDuration);
                },
                onRetryAsync: (result, timeSpan, retryCount, _) =>
                {
                    Exception(logger, result.Result, retryCount, timeSpan, result.Exception);
                    return Task.CompletedTask;
                });
    }
}