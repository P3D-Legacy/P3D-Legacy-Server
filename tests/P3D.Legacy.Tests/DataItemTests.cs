using NUnit.Framework;

using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Client.P3D;

using System.Buffers;
using System.Numerics;
using System.Text;

namespace P3D.Legacy.Tests
{
    internal sealed partial record Container() : P3DPacket(P3DPacketType.NotUsed)
    {
        [P3DPacketDataItem(0, DataItemType.String)]
        public string Name { get; set; }
        [P3DPacketDataItem(1, DataItemType.Bool)]
        public bool IsCorrect { get; set; }
        [P3DPacketDataItem(2, DataItemType.UInt64)]
        public ulong IdLong { get; set; }
        [P3DPacketDataItem(3, DataItemType.Char)]
        public char Char { get; set; }
        [P3DPacketDataItem(4, DataItemType.Int32)]
        public int IdInt { get; set; }
        [P3DPacketDataItem(5, DataItemType.String)]
        private string Position { get => DataItemStorage.Get(5); set => DataItemStorage.Set(5, value); }

        public Vector3 GetPosition() => Vector3Extensions.FromP3DString(Position, '.');
        public void SetPosition(Vector3 position) => Position = position.ToP3DString('.');
    }

    public class DataItemTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestBasic()
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

            var packetWriter = new DefaultP3DPacketWriter();
            var writer = new ArrayBufferWriter<byte>();
            packetWriter.WriteData(packet, writer);
            Assert.AreEqual("0.5|1|0|6|0|6|7|21|22|27|Aragas115124000000000j151241|1|1\r\n", Encoding.ASCII.GetString(writer.WrittenSpan));
        }
    }
}