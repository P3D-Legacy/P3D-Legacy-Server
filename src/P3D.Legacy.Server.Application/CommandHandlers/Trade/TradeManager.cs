﻿using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Application.Commands.Trade;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Trade
{
    internal sealed class TradeManager :
        ICommandHandler<TradeOfferCommand>,
        ICommandHandler<TradeAcceptCommand>,
        ICommandHandler<TradeAbortCommand>,
        ICommandHandler<TradeConfirmCommand>
    {
        private enum TradeStateTypes { WaitingForAccept, Accepted, OnePokemonSent, TwoPokemonSent, Finished, Aborted }
        private record TradeState(IPlayer Initiator, IPlayer Target, TradeStateTypes State);
        private record Key(Origin One, Origin Two)
        {
            public static Key Create(Origin one, Origin two) => one < two ? new Key(one, two) : new Key(two, one);
        }

        private readonly Dictionary<Key, TradeState> _currentTrades = new();

        public Task<CommandResult> Handle(TradeOfferCommand request, CancellationToken ct)
        {
            var (initiator, target) = request;

            var key = Key.Create(initiator.Origin, target.Origin);
            if (_currentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(CommandResult.Failure);

            _currentTrades.Add(key, new TradeState(initiator, target, TradeStateTypes.WaitingForAccept));
            return Task.FromResult(CommandResult.Success);
        }

        public Task<CommandResult> Handle(TradeAcceptCommand request, CancellationToken ct)
        {
            var (initiator, target) = request;

            var key = Key.Create(initiator.Origin, target.Origin);
            if (!_currentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(CommandResult.Failure);

            trade = trade.State switch
            {
                TradeStateTypes.WaitingForAccept => trade with { State = TradeStateTypes.Accepted },
                _ => trade with { State = TradeStateTypes.Aborted }
            };
            if (trade.State == TradeStateTypes.Aborted)
                return Task.FromResult(CommandResult.Failure);

            _currentTrades[key] = trade;
            return Task.FromResult(CommandResult.Success);
        }

        public Task<CommandResult> Handle(TradeAbortCommand request, CancellationToken ct)
        {
            var (player, partner) = request;

            var key = Key.Create(player.Origin, partner.Origin);
            _currentTrades.Remove(key);
            return Task.FromResult(CommandResult.Success);
        }

        public Task<CommandResult> Handle(TradeConfirmCommand request, CancellationToken ct)
        {
            var (player, partner) = request;

            var key = Key.Create(player.Origin, partner.Origin);
            if (!_currentTrades.TryGetValue(key, out var trade))
                return Task.FromResult(CommandResult.Failure);

            trade = trade.State switch
            {
                TradeStateTypes.Accepted => trade with { State = TradeStateTypes.OnePokemonSent },
                TradeStateTypes.OnePokemonSent => trade with { State = TradeStateTypes.TwoPokemonSent },
                _ => trade with { State = TradeStateTypes.Aborted }
            };
            if (trade.State == TradeStateTypes.Aborted)
                return Task.FromResult(CommandResult.Failure);

            _currentTrades[key] = trade;
            return Task.FromResult(CommandResult.Success);
        }
    }
}