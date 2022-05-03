using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.Trade
{
    public sealed record TradeOfferCommand(IPlayer Initiator, IPlayer Target) : ICommand;
}