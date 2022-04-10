using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.GUI.Views;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class TaskGUIManagerService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly UIServiceScopeFactory _scopeFactory;

        private Task? _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new();

        public TaskGUIManagerService(ILogger<TaskGUIManagerService> logger, UIServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

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
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        private Task Loop(CancellationToken ct)
        {
            try
            {
                using var cts = new CancellationTokenSource();
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
                void OnServerUIOnOnStop(object? sender, EventArgs args) => cts.Cancel();

                using var _ = _scopeFactory.CreateScope();
                Terminal.Gui.Application.Init();
                using var serverUI = _scopeFactory.GetRequiredService<ServerUI>();
                serverUI.OnStop += OnServerUIOnOnStop;
                using var runState = Terminal.Gui.Application.Begin(serverUI);
                while (!combinedCts.IsCancellationRequested)
                {
                    Terminal.Gui.Application.RunLoop(runState, false);
                }
                Terminal.Gui.Application.End(runState);
                serverUI.OnStop -= OnServerUIOnOnStop;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UI Error!");
            }
            finally
            {
                Terminal.Gui.Application.Shutdown();
            }

            return Task.CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var result = Console.ReadLine();
                    if (result is null) await Task.Delay(1000, ct);
                    if (result == "/uimode") await Loop(ct);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UI Error!");
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}