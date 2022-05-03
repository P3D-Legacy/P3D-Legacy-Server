using P3D.Legacy.Server.CQERS.Commands;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.CQERS.Behaviours.Command
{
    public class CommandPostProcessorBehavior<TCommand> : ICommandBehavior<TCommand>
        where TCommand : ICommand
    {
        private readonly IEnumerable<ICommandPostProcessor<TCommand>> _postProcessors;

        public CommandPostProcessorBehavior(IEnumerable<ICommandPostProcessor<TCommand>> postProcessors)
        {
            _postProcessors = postProcessors;
        }

        public async Task<CommandResult> Handle(TCommand command, CancellationToken cancellationToken, CommandHandlerDelegate next)
        {
            var result = await next();

            foreach (var processor in _postProcessors)
            {
                await processor.Process(command, result, cancellationToken);
            }

            return result;
        }
    }
}
