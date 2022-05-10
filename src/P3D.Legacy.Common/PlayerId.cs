using System;

namespace P3D.Legacy.Common
{
    public enum PlayerIdType { None, Name, GameJolt }
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public static PlayerId Parse(in ReadOnlySpan<char> chars)
        {
            if (chars.IndexOf(':') is var idx && idx == -1)
                return None;
            return new PlayerId(Enum.Parse<PlayerIdType>(chars.Slice(0, idx)), chars.Slice(idx + 1));
        }

        public static PlayerId None => new(PlayerIdType.None, string.Empty);
        public static PlayerId FromName(in ReadOnlySpan<char> name) => new(PlayerIdType.Name, in name);
        public static PlayerId FromGameJolt(GameJoltId gameJoltId) => new(PlayerIdType.GameJolt, gameJoltId.ToString());

        public static bool operator ==(PlayerId left, PlayerId right) => left.Equals(right);
        public static bool operator !=(PlayerId left, PlayerId right) => !(left == right);

        public bool IsEmpty => _idType == PlayerIdType.None;

        public PlayerIdType IdType => _idType;
        public string Id => _id;

        private readonly PlayerIdType _idType;
        private readonly string _id;
        private PlayerId(PlayerIdType idType, in ReadOnlySpan<char> id)
        {
            _idType = idType;
            _id = id.ToString();
        }

        public override string ToString() => $"{_idType}:{_id}";

        public bool Equals(PlayerId other) => _idType == other._idType && string.Equals(_id, other._id, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is Origin other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_idType, _id);

        public void Deconstruct(out PlayerIdType idType, out string id)
        {
            idType = _idType;
            id = _id;
        }
    }
}