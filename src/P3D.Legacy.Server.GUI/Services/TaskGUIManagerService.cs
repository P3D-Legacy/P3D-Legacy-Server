using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.GUI.Views;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GUI.Services
{
    public sealed class TaskGUIManagerService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly UIServiceScopeFactory _scopeFactory;

        public TaskGUIManagerService(ILogger<TaskGUIManagerService> logger, UIServiceScopeFactory scopeFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
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

        protected override async Task ExecuteAsync(CancellationToken ct)
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
    }
}