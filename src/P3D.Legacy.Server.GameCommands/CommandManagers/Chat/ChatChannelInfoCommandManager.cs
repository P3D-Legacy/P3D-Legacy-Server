/*
using MediatR;

using P3D.Legacy.Server.Abstractions;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.Managers.Chat
{
    internal class ChatChannelInfoCommandManager : CommandManager
    {
        public override string Name => "chatchannelinfo";
        public override string Description => "Get Chat Channel Info.";
        public override IEnumerable<string> Aliases => new[] { "channelinfo", "chati", "chani", "ci" };
        public override Permissions Permissions => Permissions.UserOrHigher;

        public ChatChannelInfoCommandManager(IMediator mediator) : base(mediator) { }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                var channelName = arguments[0].ToLower();
                var channel = ChatChannelManager.FindByAlias(channelName);
                await SendMessage(client, channel != null ? $"{channel.Name}: {channel.Description}" : $"Channel '{channelName}' not found!", ct);
            }
            else
                await SendMessage(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessage(client, $"Correct usage is /{alias} <global|local|<custom>>", ct);
        }
    }
}
*/