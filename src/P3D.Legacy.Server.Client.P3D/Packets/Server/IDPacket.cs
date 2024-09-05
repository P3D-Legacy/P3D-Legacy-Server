using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Client.P3D.Packets.Server;

public sealed record IdPacket() : P3DPacket(P3DPacketType.Id)
{
    public Origin PlayerOrigin { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }

    public void Deconstruct(out Origin playerOrigin)
    {
        playerOrigin = PlayerOrigin;
    }
}