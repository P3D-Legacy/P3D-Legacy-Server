using Bedrock.Framework.Protocols;

using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Client.P3D.Packets;

using System;
using System.Buffers;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D;

public sealed partial class P3DProtocol : IMessageReader<P3DPacket?>, IMessageWriter<P3DPacket?>
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "Line is not a P3D packet! Invalid protocol! {Line}")]
    private partial void InvalidProtocol(string line);
    [LoggerMessage(Level = LogLevel.Critical, Message = "Line is not a P3D packet! Invalid Packet Id! {Line}")]
    private partial void InvalidPacketId(string line);
    [LoggerMessage(Level = LogLevel.Critical, Message = "Line is not a P3D packet! Packet Id Out of Range! {Line}")]
    private partial void InvalidPacketIdOutOfRange(string line);
    [LoggerMessage(Level = LogLevel.Critical, Message = "Failed to populate message type {Type}")]
    private partial void FailedToPopulate(Type type);
    [LoggerMessage(Level = LogLevel.Trace, Message = "Received a message type {Type}")]
    private partial void ReceivedAMessage(Type type);
    [LoggerMessage(Level = LogLevel.Trace, Message = "Sending a message type {Type}")]
    private partial void SendingAMessage(Type type);


    private static readonly byte[] Separator = "|"u8.ToArray();
    private static readonly byte[] DataItemSeparator = "|0|"u8.ToArray();
    private static readonly byte[] NewLine = "\r\n"u8.ToArray();

    private readonly ILogger _logger;
    private readonly IP3DPacketFactory _packetFactory;

    public P3DProtocol(ILogger<P3DProtocol> logger, IP3DPacketFactory packetFactory)
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
        if (!P3DPacket.TryParseProtocol(ref lineSpanToParse, out var protocol) || protocol != ProtocolVersion.V1)
        {
            InvalidProtocol(Encoding.UTF8.GetString(line));
            message = null;
            return false;
        }

        if (!P3DPacket.TryParseId(ref lineSpanToParse, out var id))
        {
            InvalidPacketId(Encoding.UTF8.GetString(line));
            message = null;
            return false;
        }

        if (_packetFactory.GetFromId(id) is not { } packet)
        {
            InvalidPacketIdOutOfRange(Encoding.UTF8.GetString(line));
            message = null;
            return false;
        }

        if (!packet.TryPopulateData(ref lineSpanToParse))
        {
            FailedToPopulate(packet.GetType());
            message = null;
            return false;
        }

        ReceivedAMessage(packet.GetType());
        message = packet;
        return true;
    }

    public void WriteMessage(P3DPacket? message, IBufferWriter<byte> output)
    {
        if (message is null) return;

        SendingAMessage(message.GetType());

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

            output.WriteItems(message.DataItemStorage);
        }

        output.Write(NewLine);
    }
}