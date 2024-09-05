using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Server.Client.P3D.Packets.Server;

public sealed record WorldDataPacket() : P3DPacket(P3DPacketType.WorldData)
{
    public WorldSeason Season { get => (WorldSeason) DataItemStorage.GetInt32(0); init => DataItemStorage.Set(0, (int) value); }
    public WorldWeather Weather { get => (WorldWeather) DataItemStorage.GetInt32(1); init => DataItemStorage.Set(1, (int) value); }
    public string CurrentTime { get => DataItemStorage.Get(2); init => DataItemStorage.Set(2, value); }

    public void Deconstruct(out WorldSeason season, out WorldWeather weather, out string currentTime)
    {
        season = Season;
        weather = Weather;
        currentTime = CurrentTime;
    }
}