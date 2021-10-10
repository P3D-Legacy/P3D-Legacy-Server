using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;

using Nerdbank.Streams;

using P3D.Legacy.Common.Packets;

using System;
using System.Buffers;
using System.Text;

namespace P3D.Legacy.Server
{
    public class P3DProtocol : IMessageReader<P3DPacket?>, IMessageWriter<P3DPacket>
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
            message = null;

            _sequenceTextReader.Initialize(input, Encoding.ASCII);
            while (_sequenceTextReader.ReadLine() is { } line)
            {
                consumed = examined = input.GetPosition(Encoding.ASCII.GetByteCount(line) + NewLineLength, consumed);

                if (P3DPacket.TryParseId(line, out var id))
                {
                    message = _packetFactory.GetFromId(id);
                    if (message is null)
                    {
                        _logger.LogCritical("Line is not a P3D packet! {Line}", line);
                        continue;
                    }

                    _logger.LogInformation("Received a message type {Type}", message.GetType());
                    message.TryParseData(line);
                }
                else
                {
                    _logger.LogCritical("Line is not a P3D packet! {Line}", line);
                }
            }

            return true;
        }

        public void WriteMessage(P3DPacket message, IBufferWriter<byte> output)
        {
            _logger.LogInformation("Sending a message type {Type}", message.GetType());
            output.Write(Encoding.ASCII.GetBytes($"{message.CreateData()}\r\n"));
        }
    }
}