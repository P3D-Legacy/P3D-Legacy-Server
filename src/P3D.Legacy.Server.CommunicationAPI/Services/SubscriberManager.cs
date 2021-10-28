using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Server.CommunicationAPI.Services
{
    public class SubscriberManager
    {
        private readonly HashSet<WebSocketWrapper> _connections = new();

        public void Add(WebSocketWrapper webSocketWrapper)
        {
            _connections.Add(webSocketWrapper);
        }

        public void Remove(WebSocketWrapper webSocketWrapper)
        {
            _connections.Remove(webSocketWrapper);
        }

        public IEnumerable<WebSocketWrapper> GetActive() => _connections.ToHashSet();
    }
}
