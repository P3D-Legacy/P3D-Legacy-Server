﻿using MediatR;

using P3D.Legacy.Common.Data;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.GameCommands.CommandManagers.World
{
    public class SetSeasonCommandManager : CommandManager
    {
        public override string Name => "setseason";
        public override string Description => "Set World Season.";
        public override IEnumerable<string> Aliases => new[] { "ss" };
        public override PermissionFlags Permissions => PermissionFlags.ModeratorOrHigher;

        private readonly WorldService _worldService;

        public SetSeasonCommandManager(IMediator mediator, WorldService worldService) : base(mediator)
        {
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
        }

        public override async Task HandleAsync(IPlayer client, string alias, string[] arguments, CancellationToken ct)
        {
            if (arguments.Length == 1)
            {
                if (Enum.TryParse(arguments[0], true, out WorldSeason season))
                {
                    _worldService.Season = season;
                    await SendMessageAsync(client, $"Set Season to {season}!", ct);
                }
                else
                    await SendMessageAsync(client, $"Season '{season}' not found!", ct);
            }
            else
                await SendMessageAsync(client, "Invalid arguments given.", ct);
        }

        public override async Task HelpAsync(IPlayer client, string alias, CancellationToken ct)
        {
            await SendMessageAsync(client, $"Correct usage is /{alias} <Season>", ct);
        }
    }
}