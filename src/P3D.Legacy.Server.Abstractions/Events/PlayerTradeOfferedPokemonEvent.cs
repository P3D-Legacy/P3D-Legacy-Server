using P3D.Legacy.Common;
using P3D.Legacy.Common.Data.P3DDatas;
using P3D.Legacy.Server.CQERS.Events;

namespace P3D.Legacy.Server.Abstractions.Events
{
    public sealed record PlayerTradeOfferedPokemonEvent(IPlayer Player, Origin Partner, TradeData Data) : IEvent;
}