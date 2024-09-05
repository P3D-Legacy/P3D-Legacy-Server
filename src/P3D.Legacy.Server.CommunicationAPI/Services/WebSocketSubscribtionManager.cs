using System.Collections.Concurrent;

namespace P3D.Legacy.Server.CommunicationAPI.Services;

public class WebSocketSubscribtionManager : ConcurrentDictionary<string, WebSocketHandler> { }