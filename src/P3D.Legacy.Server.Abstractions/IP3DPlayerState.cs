using P3D.Legacy.Common;

using System.Numerics;

namespace P3D.Legacy.Server.Abstractions
{
    public interface IP3DPlayerState
    {
        string GameMode { get; }
        bool IsGameJoltPlayer { get; }
        char DecimalSeparator { get; }

        string LevelFile { get; }
        Vector3 Position { get; }
        int Facing { get; }
        bool Moving { get; }

        string Skin { get; }
        string BusyType { get; }

        bool MonsterVisible { get; }
        Vector3 MonsterPosition { get; }
        string MonsterSkin { get; }
        int MonsterFacing { get; }
    }
}