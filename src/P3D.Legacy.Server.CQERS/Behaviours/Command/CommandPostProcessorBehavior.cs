using P3D.Legacy.Server.Domain.Commands;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command;

public class CommandPostProcessorBehavior<TCommand> : ICommandBehavior<TCommand>
    where TCommand : ICommand
{
    private readonly IEnumerable<ICommandPostProcessor<TCommand>> _postProcessors;

    public int Order => int.MaxValue;

    public CommandPostProcessorBehavior(IEnumerable<ICommandPostProcessor<TCommand>> postProcessors)
    {
        _postProcessors = postProcessors;
    }

    public async Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        var result = await next();

        foreach (var processor in _postProcessors)
        {
            await processor.ProcessAsync(command, result, ct);
        }

        return result;
    }
}