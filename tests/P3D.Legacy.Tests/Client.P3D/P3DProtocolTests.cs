using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Client.P3D;

using System;
using System.Buffers;
using System.Numerics;
using System.Text;

namespace P3D.Legacy.Tests.Client.P3D
{
    internal sealed class P3DProtocolTests
    {
        private sealed class TestPacketFactory : IP3DPacketFactory
        {
            public P3DPacket? GetFromId(P3DPacketType id) => new P3DPacketSample();
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

            var packet = new P3DPacketSample
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

            var protocol = new P3DProtocol(NullLogger<P3DProtocol>.Instance, new TestPacketFactory());
            var writer = new ArrayBufferWriter<byte>();
            protocol.WriteMessage(packet, writer);
            Assert.AreEqual("0.5|1|0|6|0|6|7|21|22|27|Aragas115124000000000j151241|1|1\r\n", Encoding.ASCII.GetString(writer.WrittenSpan));

            var pos = default(SequencePosition);
            protocol.TryParseMessage(new ReadOnlySequence<byte>(writer.WrittenMemory), ref pos, ref pos, out var packet2);
            Assert.AreEqual(packet, packet2);
        }
    }
}