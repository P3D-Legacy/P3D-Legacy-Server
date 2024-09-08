using P3D.Legacy.Server.Domain.Commands;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public class CommandPreProcessorBehavior<TCommand> : ICommandBehavior<TCommand>
    where TCommand : ICommand
{
    private readonly IEnumerable<ICommandPreProcessor<TCommand>> _preProcessors;

    public int Order => int.MinValue;

    public CommandPreProcessorBehavior(IEnumerable<ICommandPreProcessor<TCommand>> preProcessors)
    {
        _preProcessors = preProcessors;
    }

    public async Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        foreach (var processor in _preProcessors)
        {
            await processor.ProcessAsync(command, ct);
        }
        return await next();
    }
}