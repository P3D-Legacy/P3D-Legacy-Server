using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets;

using System;
using System.Buffers;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed class P3DProtocol : IMessageReader<P3DPacket?>, IMessageWriter<P3DPacket>
    {
        private static readonly int NewLineLength = Encoding.ASCII.GetByteCount(new[] { '\r', '\n' });

        private readonly ILogger _logger;
        private readonly P3DPacketFactory _packetFactory;
        private readonly SequenceTextReader _sequenceTextReader = new();

        public P3DProtocol(ILogger<P3DProtocol> logger, P3DPacketFactory packetFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _packetFactory = packetFactory ?? throw new ArgumentNullException(nameof(packetFactory));
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out P3DPacket? message)
        {
            _sequenceTextReader.Initialize(input, Encoding.ASCII);
            if (_sequenceTextReader.ReadLine() is not { } line)
            {
                message = null;
                return false;
            }

            consumed = examined = input.GetPosition(Encoding.ASCII.GetByteCount(line) + NewLineLength, consumed);

            var sequence = new ReadOnlySequence<char>(line.AsMemory());
            var position = default(SequencePosition);

            if (!P3DPacket.TryParseProtocol(in sequence, ref position, out var protocol) || protocol != ProtocolEnum.V1)
            {
                _logger.LogCritical("Line is not a P3D packet! Invalid protocol! {Line}", line);
                message = null;
                return false;
            }

            if (!P3DPacket.TryParseId(in sequence, ref position, out var id))
            {
                _logger.LogCritical("Line is not a P3D packet! {Line}", line);
                message = null;
                return false;
            }

            if (_packetFactory.GetFromId(id) is not { } packet)
            {
                _logger.LogCritical("Line is not a P3D packet! Invalid Packet Id! {Line}", line);
                message = null;
                return false;
            }

            if (!packet.TryPopulateData(in sequence, ref position))
            {
                _logger.LogCritical("Failed to populate message type {Type}", packet.GetType());
                message = null;
                return false;
            }

            _logger.LogTrace("Received a message type {Type}", packet.GetType());
            message = packet;
            return true;
        }

        public void WriteMessage(P3DPacket message, IBufferWriter<byte> output)
        {
            _logger.LogTrace("Sending a message type {Type}", message.GetType());
            output.Write(Encoding.ASCII.GetBytes($"{message.CreateData()}\r\n"));
        }
    }
}