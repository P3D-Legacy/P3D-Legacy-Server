namespace P3D.Legacy.Server.Client.P3D.Packets.Server;

public sealed record ServerMessagePacket() : P3DPacket(P3DPacketType.ServerMessage)
{
    public string Message { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }

    public void Deconstruct(out string message)
    {
        message = Message;
    }
}