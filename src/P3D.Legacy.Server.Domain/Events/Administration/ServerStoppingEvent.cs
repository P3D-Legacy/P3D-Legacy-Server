namespace P3D.Legacy.Server.Domain.Events.Administration;

public sealed record ServerStoppingEvent(string Reason) : IEvent;