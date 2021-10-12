using P3D.Legacy.Common.Extensions;

using System.Numerics;

namespace P3D.Legacy.Common.Packets.Shared
{
    public sealed record GameDataPacket() : P3DPacket(P3DPacketType.GameData)
    {
        public string GameMode { get => DataItemStorage.Get(0); init => DataItemStorage.Set(0, value); }
        public bool IsGameJoltPlayer { get => DataItemStorage.GetBool(1); init => DataItemStorage.SetBool(1, value); }
        public ulong GameJoltId { get => DataItemStorage.GetUInt64(2); init => DataItemStorage.SetUInt64(2, value); }
        public char DecimalSeparator { get => DataItemStorage.GetChar(3); init => DataItemStorage.SetChar(3, value); }
        public string Name { get => DataItemStorage.Get(4); init => DataItemStorage.Set(4, value); }
        public string LevelFile { get => DataItemStorage.Get(5); init => DataItemStorage.Set(5, value); }
        public Vector3 Position { get => Vector3Extensions.FromP3DString(DataItemStorage.Get(6), DecimalSeparator); set => DataItemStorage.Set(6, value.ToP3DString(DecimalSeparator)); }
        public int Facing { get => DataItemStorage.GetInt32(7); init => DataItemStorage.SetInt32(7, value); }
        public bool Moving { get => DataItemStorage.GetBool(8); init => DataItemStorage.SetBool(8, value); }
        public string Skin { get => DataItemStorage.Get(9); init => DataItemStorage.Set(9, value); }
        public string BusyType { get => DataItemStorage.Get(10); init => DataItemStorage.Set(10, value); }
        public bool MonsterVisible { get => DataItemStorage.GetBool(11); init => DataItemStorage.SetBool(11, value); }
        public Vector3 MonsterPosition { get => Vector3Extensions.FromP3DString(DataItemStorage.Get(12), DecimalSeparator); init => DataItemStorage.Set(12, value.ToP3DString(DecimalSeparator)); }
        public string MonsterSkin { get => DataItemStorage.Get(13); init => DataItemStorage.Set(13, value); }
        public int MonsterFacing { get => DataItemStorage.GetInt32(14); init => DataItemStorage.SetInt32(14, value); }

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