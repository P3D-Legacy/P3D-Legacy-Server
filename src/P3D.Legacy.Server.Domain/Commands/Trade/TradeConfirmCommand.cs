namespace P3D.Legacy.Server.Domain.Commands.Trade;

public sealed record TradeConfirmCommand(IPlayer Player, IPlayer Partner) : ICommand;