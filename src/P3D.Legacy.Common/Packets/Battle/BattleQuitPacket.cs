﻿namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleQuitPacket() : P3DPacket(P3DPacketType.BattleQuit)
    {
        public int DestinationPlayerId { get => DataItemStorage.GetInt32(0); init => DataItemStorage.SetInt32(0, value); }

        public void Deconstruct(out int destinationPlayerId)
        {
            destinationPlayerId = DestinationPlayerId;
        }
    }
}