using MediatR;

using P3D.Legacy.Server.Abstractions.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Services
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IMediator _mediator;

        public CommandDispatcher(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public Task<CommandResult> DispatchAsync<TCommand>(TCommand command, CancellationToken ct) where TCommand : ICommand
        {
            return _mediator.Send(command, ct);
        }
    }
}