﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Commands;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Infrastructure.Models.Users;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Services.Users;

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PlayerAuthenticateDefaultCommandHandler : ICommandHandler<PlayerAuthenticateDefaultCommand>
    {
        private readonly ILogger _logger;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IUserManager _userManager;
        private readonly LockoutOptions _lockoutOptions;

        public PlayerAuthenticateDefaultCommandHandler(
            ILogger<PlayerAuthenticateDefaultCommandHandler> logger,
            INotificationDispatcher notificationDispatcher,
            IUserManager userManager,
            IOptions<LockoutOptions> lockoutOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _notificationDispatcher = notificationDispatcher ?? throw new ArgumentNullException(nameof(notificationDispatcher));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _lockoutOptions = lockoutOptions.Value ?? throw new ArgumentNullException(nameof(lockoutOptions));
        }

        public async Task<CommandResult> Handle(PlayerAuthenticateDefaultCommand request, CancellationToken ct)
        {
            var (player, password) = request;

            Debug.Assert(player.State == PlayerState.Authentication);

            if (await _userManager.FindByIdAsync(player.Id, ct) is not { } user)
            {
                user = new UserEntity(player.Id, player.Name);
                var createResult = await _userManager.CreateAsync(user, password, true, ct);
                if (createResult is not { Succeeded: true })
                {
                    foreach (var error in createResult.Errors)
                    {
                        await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, error.Description), ct);
                    }

                    return new CommandResult(false);
                }
            }

            var checkResult = await _userManager.CheckPasswordSignInAsync(user, password, true, true, ct);
            if (checkResult is not { Succeeded: true })
            {
                if (checkResult.IsLockedOut)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user, ct) ?? DateTimeOffset.Now;
                    var duration = lockoutEnd - DateTimeOffset.Now;

                    await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, $"You are locked out for {duration.Seconds} seconds!"), ct);
                    return new CommandResult(false);
                }

                var failedCount = await _userManager.GetAccessFailedCountAsync(user, ct);
                var attemptsLeft = _lockoutOptions.MaxFailedAccessAttempts - failedCount;

                await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, "Invalid Password!"), ct);
                await _notificationDispatcher.DispatchAsync(new MessageToPlayerNotification(IPlayer.Server, player, $"You have {attemptsLeft} attempts left before lockout!"), ct);
                return new CommandResult(false);
            }

            return new CommandResult(true);
        }
    }
}