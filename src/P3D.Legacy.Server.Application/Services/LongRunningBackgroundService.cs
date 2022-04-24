using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public abstract class LongRunningBackgroundService : IHostedService, IDisposable
    {
        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            _executingTask = Task.Factory.StartNew(() => ExecuteAsync(_stoppingCts.Token), _stoppingCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted)
            {
                return _executingTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            }

        }

        protected abstract Task ExecuteAsync(CancellationToken ct);

        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}