using System;

namespace P3D.Legacy.Common
{
    public enum PlayerIdType { None, Name, GameJolt, }

    public readonly record struct PlayerId(PlayerIdType IdType, string Id)
    {
        public static PlayerId Parse(in ReadOnlySpan<char> chars) => chars.IndexOf(':') is var idx && idx == -1
            ? None
            : new PlayerId(Enum.Parse<PlayerIdType>(chars.Slice(0, idx)), chars.Slice(idx + 1).ToString());

        public static PlayerId None => new(PlayerIdType.None, string.Empty);
        public static PlayerId FromName(in ReadOnlySpan<char> name) => new(PlayerIdType.Name, name.ToString());
        public static PlayerId FromGameJolt(GameJoltId gameJoltId) => new(PlayerIdType.GameJolt, gameJoltId.ToString());

        public string NameOrEmpty => IdType == PlayerIdType.Name ? Id : string.Empty;
        public GameJoltId GameJoltIdOrNone => IdType == PlayerIdType.GameJolt ? GameJoltId.Parse(Id) : GameJoltId.None;

        public bool IsNone => IdType == PlayerIdType.None;
        private string Id { get; } = Id;

        public override string ToString() => $"{IdType}:{Id}";
    }
}