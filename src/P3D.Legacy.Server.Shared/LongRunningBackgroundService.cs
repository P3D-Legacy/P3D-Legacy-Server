using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Shared;

public abstract class LongRunningBackgroundService : IHostedService, IDisposable
{
    private Task? _executingTask;
    private readonly CancellationTokenSource _stoppingCts = new();

    public Task StartAsync(CancellationToken ct)
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

    public async Task StopAsync(CancellationToken ct)
    {
        // Stop called without start
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            // Signal cancellation to the executing method
            await _stoppingCts.CancelAsync();
        }
        finally
        {
            // Wait until the task completes or the stop token triggers
#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, ct));
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
        }

    }

    protected abstract Task ExecuteAsync(CancellationToken ct);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stoppingCts.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}