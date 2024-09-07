namespace P3D.Legacy.Server.Domain.Events.Administration;

public record ServerMessageEvent(string Message) : IEvent;