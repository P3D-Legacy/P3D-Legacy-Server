using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets;
using P3D.Legacy.Server.Client.P3D.Extensions;

using System;
using System.Buffers;
using System.Text;

using ZLogger;

namespace P3D.Legacy.Server.Client.P3D
{
    public sealed class P3DProtocol : IMessageReader<P3DPacket?>, IMessageWriter<P3DPacket?>
    {
        private static readonly byte[] Separator = { (byte) '|' };
        private static readonly byte[] DataItemSeparator = { (byte) '|', (byte) '0', (byte)  '|' };
        private static readonly byte[] NewLine = { (byte) '\r', (byte) '\n' };

        private readonly ILogger _logger;
        private readonly P3DPacketFactory _packetFactory;

        public P3DProtocol(ILogger<P3DProtocol> logger, P3DPacketFactory packetFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _packetFactory = packetFactory ?? throw new ArgumentNullException(nameof(packetFactory));
        }

        public bool TryParseMessage(in ReadOnlySequence<byte> input, ref SequencePosition consumed, ref SequencePosition examined, out P3DPacket? message)
        {
            var reader = new SequenceReader<byte>(input);
            if (!reader.TryReadTo(out ReadOnlySequence<byte> line, NewLine))
            {
                message = null;
                return false;
            }
            consumed = examined = reader.Position;

            var lineSpanToParse = line;
            if (!P3DPacket.TryParseProtocol(ref lineSpanToParse, out var protocol) || protocol != ProtocolEnum.V1)
            {
                _logger.ZLogCritical("Line is not a P3D packet! Invalid protocol! {Line}", Encoding.UTF8.GetString(line));
                message = null;
                return false;
            }

            if (!P3DPacket.TryParseId(ref lineSpanToParse, out var id))
            {
                _logger.ZLogCritical("Line is not a P3D packet! Invalid Packet Id! {Line}", Encoding.UTF8.GetString(line));
                message = null;
                return false;
            }

            if (_packetFactory.GetFromId(id) is not { } packet)
            {
                _logger.ZLogCritical("Line is not a P3D packet! Packet Id Out of Range! {Line}", Encoding.UTF8.GetString(line));
                message = null;
                return false;
            }

            if (!packet.TryPopulateData(ref lineSpanToParse))
            {
                _logger.ZLogCritical("Failed to populate message type {Type}", packet.GetType());
                message = null;
                return false;
            }

            _logger.ZLogTrace("Received a message type {Type}", packet.GetType());
            message = packet;
            return true;
        }

        public void WriteMessage(P3DPacket? message, IBufferWriter<byte> output)
        {
            if (message is null) return;

            _logger.ZLogTrace("Sending a message type {Type}", message.GetType());

            output.Write(message.Protocol);
            output.Write(Separator);
            output.WriteAsText((byte) message.Id);
            output.Write(Separator);
            output.Write(message.Origin);

            if (message.DataItemStorage.Count == 0)
            {
                output.Write(DataItemSeparator);
            }
            else
            {
                output.Write(Separator);
                output.WriteAsText(message.DataItemStorage.Count);
                output.Write(DataItemSeparator);

                // We can't switch to a better implementation where
                // we will write DataItems without allocating
                // the whole buffer before. This 'header' information
                // requires us to know the whole build datastructure

                for (int i = 0, pos = 0; i < message.DataItemStorage.Count - 1; i++)
                {
                    // We skip writing 0, it's obvious. Start with the second item
                    pos += message.DataItemStorage.Get(i).Length;
                    output.WriteAsText(pos);
                    output.Write(Separator);
                }

                var encoder = Encoding.ASCII.GetEncoder();
                foreach (var dataItem in message.DataItemStorage)
                {
                    output.Write(dataItem, encoder);
                }
            }

            output.Write(NewLine);
        }
    }
}