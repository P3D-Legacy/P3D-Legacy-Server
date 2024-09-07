using P3D.Legacy.Server.CQERS.Behaviours.Command;
using P3D.Legacy.Server.Domain.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Services;

internal sealed class CommandDispatcherHelper<TCommand> where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _handler;
    private readonly IEnumerable<ICommandBehavior<TCommand>> _behaviors;

    public CommandDispatcherHelper(ICommandHandler<TCommand> handler, IEnumerable<ICommandBehavior<TCommand>> behaviors)
    {
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _behaviors = behaviors ?? throw new ArgumentNullException(nameof(behaviors));
    }

    public Task<CommandResult> DispatchAsync(TCommand command, CancellationToken ct)
    {
        Task<CommandResult> GetHandlerAsync() => _handler.HandleAsync(command, ct);
        return _behaviors
            .Reverse()
            .Aggregate((CommandHandlerDelegate) GetHandlerAsync, (next, pipeline) => () => pipeline.HandleAsync(command, next, ct))();
    }
}