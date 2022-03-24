using P3D.Legacy.Common;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Services
{
    public enum TradeStateEnum { WaitingForAccept, Accepted, OnePokemonSent, TwoPokemonSent, Finished, Aborted }
    public record TradeState(IPlayer Initiator, IPlayer Target, TradeStateEnum State);

    public class TradeManager
    {
        private record Key(Origin One, Origin Two)
        {
            public static Key Create(Origin one, Origin two) => one < two ? new Key(one, two) : new Key(two, one);
        }

        private readonly Dictionary<Key, TradeState> CurrentTrades = new();

        public Task<bool> OfferTrade(IPlayer initiator, IPlayer target)
        {
            var key = Key.Create(initiator.Origin, target.Origin);
            if (CurrentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(false);

            CurrentTrades.Add(key, new TradeState(initiator, target, TradeStateEnum.WaitingForAccept));
            return Task.FromResult(true);
        }

        public Task<bool> AcceptTrade(IPlayer initiator, IPlayer target)
        {
            var key = Key.Create(initiator.Origin, target.Origin);
            if (!CurrentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(false);

            trade = trade.State switch
            {
                TradeStateEnum.WaitingForAccept => trade with { State = TradeStateEnum.Accepted },
                _ => trade with { State = TradeStateEnum.Aborted }
            };
            if (trade.State == TradeStateEnum.Aborted)
                return Task.FromResult(false);

            CurrentTrades[key] = trade;
            return Task.FromResult(true);
        }

        public Task AbortTrade(IPlayer player, IPlayer partner)
        {
            var key = Key.Create(player.Origin, partner.Origin);
            CurrentTrades.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ConfirmTrade(IPlayer player, IPlayer partner)
        {
            var key = Key.Create(player.Origin, partner.Origin);
            if (!CurrentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(false);

            trade = trade.State switch
            {
                TradeStateEnum.Accepted => trade with { State = TradeStateEnum.OnePokemonSent },
                TradeStateEnum.OnePokemonSent => trade with { State = TradeStateEnum.TwoPokemonSent },
                _ => trade with { State = TradeStateEnum.Aborted }
            };
            if (trade.State == TradeStateEnum.Aborted)
                return Task.FromResult(false);

            CurrentTrades[key] = trade;
            return Task.FromResult(true);
        }
    }
}