using P3D.Legacy.Common.PlayerEvents;

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
        Depreceated1 = 0x05,
        Kicked = 0x06,
        PlayerTriggeredEvent = 0x07,

        Error = 0xFF,
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(PlayerJoinedResponsePayload), typeDiscriminator: (int) ResponsePayloadType.PlayerJoined)]
    [JsonDerivedType(typeof(PlayerLeftResponsePayload), typeDiscriminator: (int) ResponsePayloadType.PlayerLeft)]
    [JsonDerivedType(typeof(PlayerSentGlobalMessageResponsePayload), typeDiscriminator: (int) ResponsePayloadType.PlayerSentGlobalMessage)]
    [JsonDerivedType(typeof(ServerMessageResponsePayload), typeDiscriminator: (int) ResponsePayloadType.ServerMessage)]
    [JsonDerivedType(typeof(PlayerTriggeredEventResponsePayload), typeDiscriminator: (int) ResponsePayloadType.PlayerTriggeredEvent)]
    [JsonDerivedType(typeof(KickedResponsePayload), typeDiscriminator: (int) ResponsePayloadType.Kicked)]
    [JsonDerivedType(typeof(SuccessResponsePayload), typeDiscriminator: (int) ResponsePayloadType.Success)]
    [JsonDerivedType(typeof(ErrorResponsePayload), typeDiscriminator: (int) ResponsePayloadType.Error)]
    internal abstract record ResponsePayload([property: JsonIgnore] ResponsePayloadType Type, long Timestamp);
    internal sealed record PlayerJoinedResponsePayload(string Player) : ResponsePayload(ResponsePayloadType.PlayerJoined, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerLeftResponsePayload(string Player) : ResponsePayload(ResponsePayloadType.PlayerLeft, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerSentGlobalMessageResponsePayload(string Player, string Message) : ResponsePayload(ResponsePayloadType.PlayerSentGlobalMessage, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record ServerMessageResponsePayload(string Message) : ResponsePayload(ResponsePayloadType.ServerMessage, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record PlayerTriggeredEventResponsePayload(string Player, PlayerEvent Event) : ResponsePayload(ResponsePayloadType.PlayerTriggeredEvent, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record KickedResponsePayload(string Reason) : ResponsePayload(ResponsePayloadType.Kicked, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record SuccessResponsePayload(Guid Uid) : ResponsePayload(ResponsePayloadType.Success, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    internal sealed record ErrorResponsePayload(int Code, string Message, Guid Uid) : ResponsePayload(ResponsePayloadType.Error, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
}