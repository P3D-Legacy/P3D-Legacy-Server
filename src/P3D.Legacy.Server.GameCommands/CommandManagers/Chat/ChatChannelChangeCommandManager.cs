/*
using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Chat
{
    public class ChatChannelChangeCommandManager : CommandManager
    {
        public override string Name => "chatchannelchange";
        public override string Description => "Change Clients Chat Channel.";
        public override IEnumerable<string> Aliases => new [] { "channelchange", "chatc", "chanc", "cc" };
        public override Permissions Permissions => Permissions.UserOrHigher ^ Permissions.Server;

        public ChatChannelChangeCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var channelName = arguments[0].ToLower();
                var channel = ChatChannelManager.FindByAlias(channelName);
                if(channel != null)
                    await SendMessage(client, channel.Subscribe(client) ? $"Changed chat channel to {channel.Name}!" : $"Failed to change chat channel to {channel.Name}!", ct);
                else
                    await SendMessage(client, $"Channel '{channelName}' not found!", ct);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} <global/local/'custom'>", ct);
        }
    }
}
*/