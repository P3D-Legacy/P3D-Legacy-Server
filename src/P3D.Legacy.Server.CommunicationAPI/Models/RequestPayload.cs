using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.CommunicationAPI.Models
{
    internal enum RequestPayloadType
    {
        RegisterBot = 0x01,
        MessageRequest = 0x02
    }

#pragma warning disable SYSLIB1037
    internal abstract partial record RequestPayload([JsonDiscriminator] RequestPayloadType Type, Guid Uid);
    [SuppressMessage("Performance", "CA1812")]
    internal sealed record RegisterBotRequestPayload(string BotName, Guid Uid) : RequestPayload(RequestPayloadType.RegisterBot, Uid);
    [SuppressMessage("Performance", "CA1812")]
    internal sealed record MessageRequestPayload(string Sender, string Message, Guid Uid) : RequestPayload(RequestPayloadType.MessageRequest, Uid);
#pragma warning restore SYSLIB1037
}