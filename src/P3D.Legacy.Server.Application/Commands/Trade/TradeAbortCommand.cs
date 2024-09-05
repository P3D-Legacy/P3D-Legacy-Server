using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.CQERS.Commands;

namespace P3D.Legacy.Server.Application.Commands.Trade;

public sealed record TradeAbortCommand(IPlayer Player, IPlayer Partner) : ICommand;