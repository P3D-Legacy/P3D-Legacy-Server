using P3D.Legacy.Common.Extensions;

using System.Numerics;

namespace P3D.Legacy.Common.Packets.Shared
{
    public sealed partial record GameDataPacket() : P3DPacket(P3DPacketType.GameData)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string GameMode { get; set; }
        [P3DPacketDataItem(1, DataItemType.Bool)]
        public bool IsGameJoltPlayer { get; set; }
        [P3DPacketDataItem(2, DataItemType.UInt64)]
        public ulong GameJoltId { get; set; }
        [P3DPacketDataItem(3, DataItemType.Char)]
        public char DecimalSeparator { get; set; }
        [P3DPacketDataItem(4, DataItemType.String)]
        public string Name { get; set; }
        [P3DPacketDataItem(5, DataItemType.String)]
        public string LevelFile { get; set; }
        [P3DPacketDataItem(6, DataItemType.Vector3)]
        public Vector3 Position { get => Vector3Extensions.FromP3DString(DataItemStorage.Get(6), DecimalSeparator); set => DataItemStorage.Set(6, value.ToP3DString(DecimalSeparator)); }
        [P3DPacketDataItem(7, DataItemType.Int32)]
        public int Facing { get; set; }
        [P3DPacketDataItem(8, DataItemType.Bool)]
        public bool Moving { get; set; }
        [P3DPacketDataItem(9, DataItemType.String)]
        public string Skin { get; set; }
        [P3DPacketDataItem(10, DataItemType.String)]
        public string BusyType { get; set; }
        [P3DPacketDataItem(11, DataItemType.Bool)]
        public bool MonsterVisible { get; set; }
        [P3DPacketDataItem(12, DataItemType.Vector3)]
        public Vector3 MonsterPosition { get; set; }
        [P3DPacketDataItem(13, DataItemType.String)]
        public string MonsterSkin { get; set; }
        [P3DPacketDataItem(14, DataItemType.Int32)]
        public int MonsterFacing { get; set; }

        public void Deconstruct(
            out string gameMode,
            out bool isGameJoltPlayer,
            out ulong gameJoltId,
            out char decimalSeparator,
            out string name,
            out string levelFile,
            out Vector3 position,
            out int facing,
            out bool moving,
            out string skin,
            out string busyType,
            out bool monsterVisible,
            out Vector3 monsterPosition,
            out string monsterSkin,
            out int monsterFacing)
        {
            gameMode = GameMode;
            isGameJoltPlayer = IsGameJoltPlayer;
            gameJoltId = GameJoltId;
            decimalSeparator = DecimalSeparator;
            name = Name;
            levelFile = LevelFile;
            position = Position;
            facing = Facing;
            moving = Moving;
            skin = Skin;
            busyType = BusyType;
            monsterVisible = MonsterVisible;
            monsterPosition = MonsterPosition;
            monsterSkin = MonsterSkin;
            monsterFacing = MonsterFacing;
        }
    }
}