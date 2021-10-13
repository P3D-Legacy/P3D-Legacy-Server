using NUnit.Framework;

using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets;

using System.Numerics;

namespace P3D.Legacy.Common.Tests
{
    public class DataItemTests
    {
        private sealed record Container() : P3DPacket(P3DPacketType.NotUsed)
        {
            public string Name { get => DataItemStorage.Get(0); set => DataItemStorage.Set(0, value); }
            public bool IsCorrect { get => DataItemStorage.GetBool(1); set => DataItemStorage.Set(1, value); }
            public ulong IdLong { get => DataItemStorage.GetUInt64(2); set => DataItemStorage.Set(2, value); }
            public char Char { get => DataItemStorage.GetChar(3); set => DataItemStorage.Set(3, value); }
            public int IdInt { get => DataItemStorage.GetInt32(4); set => DataItemStorage.Set(4, value); }

            private string Position { get => DataItemStorage.Get(5); set => DataItemStorage.Set(5, value); }
            public Vector3 GetPosition() => Vector3Extensions.FromP3DString(Position, '.');
            public void SetPosition(Vector3 position) => Position = position.ToP3DString('.');
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var name = "Aragas";
            var isCorrect = true;
            var idULong = 15124000000000U;
            var @char = 'j';
            var idInt = 15124;
            var position = Vector3.One;

            var packet = new Container
            {
                Name = name,
                IsCorrect = isCorrect,
                IdLong = idULong,
                Char = @char,
                IdInt = idInt
            };
            packet.SetPosition(position);

            Assert.AreEqual(name, packet.Name);
            Assert.AreEqual(isCorrect, packet.IsCorrect);
            Assert.AreEqual(idULong, packet.IdLong);
            Assert.AreEqual(@char, packet.Char);
            Assert.AreEqual(idInt, packet.IdInt);
            Assert.AreEqual(position, packet.GetPosition());

            Assert.AreEqual("0.5|1|0|6|0|6|7|21|22|27|Aragas115124000000000j151241|1|1", packet.CreateData());
        }
    }
}