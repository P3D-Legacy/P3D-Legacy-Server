using MediatR;

namespace P3D.Legacy.Server.Abstractions.Commands
{
    public interface ICommand : ICommand<CommandResult> { }
    public interface ICommand<out TCommandResult> : IRequest<TCommandResult> { }
}