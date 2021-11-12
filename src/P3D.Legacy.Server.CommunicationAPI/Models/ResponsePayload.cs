using System;
using System.Text.Json.Serialization;

namespace P3D.Legacy.Server.CommunicationAPI.Models
{
    internal enum ResponsePayloadType
    {
        Success = 0x00,
        PlayerJoined = 0x01,
        PlayerLeft = 0x02,
        PlayerSentGlobalMessage = 0x03,
        ServerMessage = 0x04,
        PlayerTriggeredEvent = 0x05,
        Kicked = 0x06,

        Error = 0xFF
    }

    internal abstract partial record ResponsePayload([JsonDiscriminator] ResponsePayloadType Type, long Timestamp);
    internal sealed record PlayerJoinedResponsePayload(string Player) : ResponsePayload(ResponsePayloadType.PlayerJoined, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerLeftResponsePayload(string Player) : ResponsePayload(ResponsePayloadType.PlayerLeft, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerSentGlobalMessageResponsePayload(string Player, string Message) : ResponsePayload(ResponsePayloadType.PlayerSentGlobalMessage, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record ServerMessageResponsePayload(string Message) : ResponsePayload(ResponsePayloadType.ServerMessage, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerTriggeredEventResponsePayload(string Player, string Event) : ResponsePayload(ResponsePayloadType.PlayerTriggeredEvent, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record KickedResponsePayload(string Reason) : ResponsePayload(ResponsePayloadType.Kicked, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record SuccessResponsePayload(Guid Uid) : ResponsePayload(ResponsePayloadType.Success, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record ErrorResponsePayload(int Code, string Message, Guid Uid) : ResponsePayload(ResponsePayloadType.Error, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
}