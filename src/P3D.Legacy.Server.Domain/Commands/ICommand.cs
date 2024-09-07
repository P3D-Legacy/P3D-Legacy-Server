namespace P3D.Legacy.Server.Domain.Commands;

public interface IBaseCommand<out TCommandResult> { }
public interface ICommand : IBaseCommand<CommandResult> { }

//public interface ICommand<out TCommandResult> : IBaseCommand<TCommandResult> { }