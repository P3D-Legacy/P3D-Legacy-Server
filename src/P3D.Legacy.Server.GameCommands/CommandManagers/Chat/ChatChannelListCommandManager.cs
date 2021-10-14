/*
using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Chat
{
    public class ChatChannelListCommandManager : CommandManager
    {
        public override string Name => "chatchannellist";
        public override string Description => "Get all Chat Channels.";
        public override IEnumerable<string> Aliases => new [] { "channellist", "chatl", "chanl", "cl" };
        public override Permissions Permissions => Permissions.UserOrHigher;

        public ChatChannelListCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            foreach (var channel in ChatChannelManager.GetChatChannels())
                await SendMessage(client, $"{channel.Name}: {channel.Description}", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias}", ct);
        }
    }
}
*/