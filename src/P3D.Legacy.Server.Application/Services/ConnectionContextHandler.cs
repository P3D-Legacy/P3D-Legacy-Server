using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services
{
    public abstract class ConnectionContextHandler : IAsyncDisposable
    {
        public string ConnectionId => Connection.ConnectionId;
        protected ConnectionContext Connection { get; private set; } = default!;

        private CancellationTokenSource? _stoppingCts;
        private Task? _executingTask;

        public Task<ConnectionContextHandler> SetConnectionContextAsync(ConnectionContext connectionContext)
        {
            Connection = connectionContext;

            var lifetimeNotificationFeature = Connection.Features.Get<IConnectionLifetimeNotificationFeature>();
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(Connection.ConnectionClosed, lifetimeNotificationFeature?.ConnectionClosedRequested ?? CancellationToken.None);
            _executingTask = OnCreatedAsync(_stoppingCts.Token);

            return Task.FromResult(this);
        }

        protected abstract Task OnCreatedAsync(CancellationToken ct);

        public async Task ListenAsync()
        {
            try { await (_executingTask ?? Task.CompletedTask); }
            catch (Exception e) when (e is TaskCanceledException or OperationCanceledException) { }
        }

        public virtual async Task StopAsync(CancellationToken ct)
        {
            if (_stoppingCts is null || _executingTask is null)
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
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, ct));
#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks
            }

        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                _stoppingCts?.Dispose();
                _executingTask?.Dispose();
            }

            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
    }
}