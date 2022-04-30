using MediatR;

namespace P3D.Legacy.Server.Abstractions.Commands
{
    public interface ICommandHandler { }
    public interface ICommandHandler<in TCommand> : ICommandHandler, IRequestHandler<TCommand, CommandResult> where TCommand : ICommand { }
}
