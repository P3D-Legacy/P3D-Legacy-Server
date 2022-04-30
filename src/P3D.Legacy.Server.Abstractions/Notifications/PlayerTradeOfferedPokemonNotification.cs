using P3D.Legacy.Common;
using P3D.Legacy.Common.Data.P3DDatas;

namespace P3D.Legacy.Server.Abstractions.Notifications
{
    public sealed record PlayerTradeOfferedPokemonNotification(IPlayer Player, Origin Partner, TradeData Data) : INotification;
}