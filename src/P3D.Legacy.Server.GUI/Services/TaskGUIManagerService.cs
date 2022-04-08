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

        protected override Task ExecuteAsync(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Loop();

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

            return Task.CompletedTask;
        }
    }
}