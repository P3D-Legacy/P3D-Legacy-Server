using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public interface IPlayer
    {
        event Func<string, P3DConnectionHandler, Task>? InitializingAsync;
        event Func<string, P3DConnectionHandler, Task>? InitializedAsync;
        event Func<string, P3DConnectionHandler, Task>? DisconnectedAsync;

        Task AssignIdAsync(uint id);
    }

    public record PlayerInfo
    {
        public uint Id { get; init; }
        public string Name { get; init; }
    }

    public class PlayerHandlerService
    {
        private static uint _globalPlayerIncrement = 0;

        public IReadOnlyCollection<PlayerInfo> Players => _connections.Values.Select(x => new PlayerInfo() { Id = x.Id, Name = x.Name }).ToImmutableArray();
        private readonly Dictionary<string, P3DConnectionHandler> _connections = new();

        public Task AcknowledgeConnectionAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            connectionHandler.InitializingAsync += ConnectionHandler_InitializingAsync;
            connectionHandler.InitializedAsync += ConnectionHandler_InitializedAsync;
            connectionHandler.DisconnectedAsync += ConnectionHandler_DisconnectedAsync;
            return Task.CompletedTask;
        }

        private async Task ConnectionHandler_InitializingAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            await connectionHandler.AssignIdAsync(Interlocked.Increment(ref _globalPlayerIncrement));
        }
        private Task ConnectionHandler_InitializedAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            _connections[connectionId] = connectionHandler;
            return Task.CompletedTask;
        }
        private Task ConnectionHandler_DisconnectedAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            _connections.Remove(connectionId);
            return Task.CompletedTask;
        }
    }
}
