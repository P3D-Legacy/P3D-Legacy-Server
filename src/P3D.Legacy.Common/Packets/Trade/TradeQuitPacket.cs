﻿namespace P3D.Legacy.Common.Packets.Trade
{
    public sealed record TradeQuitPacket() : P3DPacket(P3DPacketType.TradeQuit)
    {
        public int DestinationPlayerId { get => DataItemStorage.GetInt32(0); init => DataItemStorage.SetInt32(0, value); }

        public void Deconstruct(out int destinationPlayerId)
        {
            destinationPlayerId = DestinationPlayerId;
        }
    }
}