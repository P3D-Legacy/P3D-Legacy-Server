using Microsoft.Extensions.Hosting;

using P3D.Legacy.Server.Abstractions.Events;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Extensions;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public sealed class ShutdownListener : LongRunningBackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IEventDispatcher _eventDispatcher;

        public ShutdownListener(IHostApplicationLifetime hostApplicationLifetime, IEventDispatcher eventDispatcher)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _eventDispatcher = eventDispatcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var waitForStop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            await using var registration = _hostApplicationLifetime.ApplicationStopping.Register(static obj =>
            {
                if (obj is not TaskCompletionSource tcs) return;
                tcs.TrySetResult();
            }, waitForStop);

            await waitForStop.Task;

            await _eventDispatcher.DispatchAsync(new ServerStoppingEvent("Server stopping!"), CancellationToken.None);
        }
    }
}