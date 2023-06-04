using Microsoft.Extensions.Hosting;

using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public sealed class ShutdownListener : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IPlayerContainerReader _playerContainer;

        public ShutdownListener(IHostApplicationLifetime hostApplicationLifetime, IPlayerContainerReader playerContainer)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _playerContainer = playerContainer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var waitForStop = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            _hostApplicationLifetime.ApplicationStopping.Register(static obj =>
            {
                if (obj is not TaskCompletionSource tcs) return;
                tcs.TrySetResult();
            }, waitForStop);

            await waitForStop.Task;

            await foreach (var player in _playerContainer.GetAllAsync(CancellationToken.None))
                await player.KickAsync("Server stopping!", CancellationToken.None);
        }
    }
}