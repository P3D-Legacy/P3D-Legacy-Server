namespace P3D.Legacy.Server.Domain.Commands.Trade;

public sealed record TradeOfferCommand(IPlayer Initiator, IPlayer Target) : ICommand;