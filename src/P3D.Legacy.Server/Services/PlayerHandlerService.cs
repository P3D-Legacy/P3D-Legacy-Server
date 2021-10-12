using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Services
{
    public record Event;
    public record PlayerJoinedEvent(string Message) : Event;

    public interface IPlayer
    {
        event Func<string, P3DConnectionHandler, Task>? OnInitializingAsync;
        event Func<string, P3DConnectionHandler, Task>? OnInitializedAsync;
        event Func<string, P3DConnectionHandler, Task>? OnDisconnectedAsync;

        ulong Id { get; }
        string Name { get; }

        Task AssignIdAsync(ulong id);
    }

    public interface IGameJoltPlayer
    {
        ulong GameJoltId { get; }
    }

    public record PlayerInfo(ulong Id, string Name, ulong GameJoltId);

    public class PlayerHandlerService
    {
        private static ulong GlobalPlayerIncrement = 0;

        public event Func<Event, Task>? OnEventAsync;

        public IReadOnlyCollection<PlayerInfo> Players => _connections.Values.Select(x => new PlayerInfo(x.Id, x.Name, x.GameJoltId)).ToImmutableArray();
        private readonly Dictionary<string, P3DConnectionHandler> _connections = new();

        public Task AcknowledgeConnectionAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            connectionHandler.OnInitializingAsync += ConnectionHandler_OnInitializingAsync;
            connectionHandler.OnInitializedAsync += ConnectionHandler_OnInitializedAsync;
            connectionHandler.OnDisconnectedAsync += ConnectionHandler_OnDisconnectedAsync;
            return Task.CompletedTask;
        }

        private async Task ConnectionHandler_OnInitializingAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            await connectionHandler.AssignIdAsync(Interlocked.Increment(ref GlobalPlayerIncrement));
        }
        private async Task ConnectionHandler_OnInitializedAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            _connections[connectionId] = connectionHandler;

            if (OnEventAsync is not null)
                await OnEventAsync(new PlayerJoinedEvent($"Player {connectionHandler.Name} joined the game!"));
        }
        private Task ConnectionHandler_OnDisconnectedAsync(string connectionId, P3DConnectionHandler connectionHandler)
        {
            _connections.Remove(connectionId);
            connectionHandler.OnInitializingAsync -= ConnectionHandler_OnInitializingAsync;
            connectionHandler.OnInitializedAsync -= ConnectionHandler_OnInitializedAsync;
            connectionHandler.OnDisconnectedAsync -= ConnectionHandler_OnDisconnectedAsync;
            return Task.CompletedTask;
        }
    }
}