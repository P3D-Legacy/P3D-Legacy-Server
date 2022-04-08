using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.GUI.Views;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class ThreadGUIManagerService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly UIServiceScopeFactory _scopeFactory;

        private Thread? _executingThread;
        private readonly TaskCompletionSource _threadCompletionSource = new();
        private readonly CancellationTokenSource _stoppingCts = new();

        public ThreadGUIManagerService(ILogger<ThreadGUIManagerService> logger, UIServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        }

        public Task StartAsync(CancellationToken ct)
        {
            _executingThread = new Thread(() => Execute(_stoppingCts.Token));
            _executingThread.Start();

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken ct)
        {
            // Stop called without start
            if (_executingThread == null)
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
                await Task.WhenAny(_threadCompletionSource.Task, Task.Delay(Timeout.Infinite, ct));
            }
        }

        private void Loop()
        {
            try
            {
                var cts = new CancellationTokenSource();

                using var _ = _scopeFactory.CreateScope();
                Terminal.Gui.Application.Init();
                using var serverUI = _scopeFactory.GetRequiredService<ServerUI>();
                serverUI.OnStop += (sender, args) => cts.Cancel();
                using var runState = Terminal.Gui.Application.Begin(serverUI);
                while (!cts.IsCancellationRequested)
                {
                    Terminal.Gui.Application.RunLoop(runState, false);
                }
                Terminal.Gui.Application.End(runState);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UI Error!");
            }
            finally
            {
                Terminal.Gui.Application.Shutdown();
            }
        }

        private void Execute(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    //Loop();

                    if (Console.ReadLine() == "/uimode")
                    {
                        Loop();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "UI Error!");
            }
            finally
            {
                _threadCompletionSource.SetResult();
            }
        }

        public void Dispose()
        {
            _stoppingCts.Cancel();
        }
    }
}