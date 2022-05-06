using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets;

using System;
using System.Numerics;

namespace P3D.Legacy.Tests.Client.P3D
{
    internal sealed record P3DPacketSample() : P3DPacket(P3DPacketType.NotUsed)
    {
        public string Name { get => DataItemStorage.Get(0); set => DataItemStorage.Set(0, value); }
        public bool IsCorrect { get => DataItemStorage.GetBool(1); set => DataItemStorage.Set(1, value); }
        public ulong IdLong { get => DataItemStorage.GetUInt64(2); set => DataItemStorage.Set(2, value); }
        public char? Char { get => DataItemStorage.GetChar(3); set => DataItemStorage.Set(3, value); }
        public int IdInt { get => DataItemStorage.GetInt32(4); set => DataItemStorage.Set(4, value); }

        private string Position { get => DataItemStorage.Get(5); set => DataItemStorage.Set(5, value); }
        public Vector3 GetPosition() => Vector3Extensions.FromP3DString(Position, '.');
        public void SetPosition(Vector3 position) => Position = position.ToP3DString('.');

        public bool Equals(P3DPacketSample? other) =>
            other is not null
            && string.Equals(Name, other.Name)
            && IsCorrect.Equals(other.IsCorrect)
            && IdLong.Equals(other.IdLong)
            && Char.Equals(other.Char)
            && IdInt.Equals(other.IdInt)
            && string.Equals(Position, other.Position);

        public override int GetHashCode() => HashCode.Combine(Name, IsCorrect, IdLong, Char, IdInt, Position);
    }
}