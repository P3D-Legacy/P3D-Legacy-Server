using System;

namespace P3D.Legacy.Common
{
    public enum PlayerIdType { None, Name, GameJolt }
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public static PlayerId Parse(string str)
        {
            var split = str.Split(':');
            return new PlayerId(Enum.Parse<PlayerIdType>(split[0]), split[1]);
        }

        public static PlayerId None => new(PlayerIdType.None, string.Empty);
        public static PlayerId FromName(string name) => new(PlayerIdType.Name, name);
        public static PlayerId FromGameJolt(GameJoltId gameJoltId) => new(PlayerIdType.GameJolt, gameJoltId.ToString());

        public static bool operator ==(PlayerId left, PlayerId right) => left.Equals(right);
        public static bool operator !=(PlayerId left, PlayerId right) => !(left == right);

        public bool IsEmpty => _idType == PlayerIdType.None;

        public PlayerIdType IdType => _idType;
        public string Id => _id;

        private readonly PlayerIdType _idType;
        private readonly string _id;
        private PlayerId(PlayerIdType idType, string id)
        {
            _idType = idType;
            _id = id;
        }

        public override string ToString() => $"{_idType}:{_id}";

        public bool Equals(PlayerId other) => _idType == other._idType && _id == other._id;
        public override bool Equals(object? obj) => obj is Origin other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(_idType, _id);
    }
}