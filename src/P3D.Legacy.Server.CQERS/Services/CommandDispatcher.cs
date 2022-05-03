using Microsoft.Extensions.DependencyInjection;

using P3D.Legacy.Server.CQERS.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services
{
    public sealed class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task<CommandResult> DispatchAsync<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand
        {
            return _serviceProvider.GetRequiredService<CommandDispatcherHelper<TCommand>>().DispatchAsync(command, ct);
        }
    }
}