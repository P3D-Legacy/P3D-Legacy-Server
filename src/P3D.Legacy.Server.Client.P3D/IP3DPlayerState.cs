using System.Numerics;

namespace P3D.Legacy.Server.Client.P3D;

public interface IP3DPlayerState
{
    private record P3DPlayerState(string GameMode, bool IsGameJoltPlayer, char DecimalSeparator, string LevelFile, Vector3 Position, int Facing, bool Moving, string Skin, string BusyType, bool MonsterVisible, Vector3 MonsterPosition, string MonsterSkin, int MonsterFacing) : IP3DPlayerState;
    private sealed record EmptyP3DPlayerState() : P3DPlayerState("", false, '.', "", Vector3.Zero, 0, false, "", "", false, Vector3.Zero, "", 0);

    static IP3DPlayerState Empty { get; } = new EmptyP3DPlayerState();

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