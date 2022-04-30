using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;

namespace P3D.Legacy.Server.Application.Commands.Trade
{
    public sealed record TradeConfirmCommand(IPlayer Player, IPlayer Partner) : ICommand;
}