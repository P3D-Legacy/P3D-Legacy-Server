using P3D.Legacy.Common.Data;

namespace P3D.Legacy.Common.Packets.Server
{
    public sealed partial record WorldDataPacket() : P3DPacket(P3DPacketType.WorldData)
    {
        [P3DPacketDataItem(0, DataItemType.Int32)]
        public WorldSeason Season { get => (WorldSeason) DataItemStorage.GetInt32(0); init => DataItemStorage.Set(0, (int) value); }
        [P3DPacketDataItem(1, DataItemType.Int32)]
        public WorldWeather Weather { get => (WorldWeather) DataItemStorage.GetInt32(1); init => DataItemStorage.Set(1, (int) value); }
        [P3DPacketDataItem(2, DataItemType.String)]
        public string CurrentTime { get => DataItemStorage.Get(2); init => DataItemStorage.Set(2, value); }

        public void Deconstruct(out WorldSeason season, out WorldWeather weather, out string currentTime)
        {
            season = Season;
            weather = Weather;
            currentTime = CurrentTime;
        }
    }
}