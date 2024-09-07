namespace P3D.Legacy.Server.Domain.Commands.Trade;

public sealed record TradeAbortCommand(IPlayer Player, IPlayer Partner) : ICommand;