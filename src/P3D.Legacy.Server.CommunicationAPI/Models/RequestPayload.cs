using System;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.CommunicationAPI.Models
{
    internal enum RequestPayloadType
    {
        RegisterBot = 0x01,
        MessageRequest = 0x02,
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(RegisterBotRequestPayload), typeDiscriminator: (int) RequestPayloadType.RegisterBot)]
    [JsonDerivedType(typeof(MessageRequestPayload), typeDiscriminator: (int) RequestPayloadType.MessageRequest)]
    internal abstract record RequestPayload(Guid Uid);
    internal sealed record RegisterBotRequestPayload(string BotName, Guid Uid) : RequestPayload(Uid);
    internal sealed record MessageRequestPayload(string Sender, string Message, Guid Uid) : RequestPayload(Uid);
}