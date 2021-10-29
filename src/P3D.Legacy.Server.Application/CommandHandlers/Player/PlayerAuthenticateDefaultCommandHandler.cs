using LiteDB;

using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Identity;
using P3D.Legacy.Server.Abstractions.Notifications;
using P3D.Legacy.Server.Application.Commands;
using P3D.Legacy.Server.Application.Commands.Player;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Player
{
    internal class PlayerAuthenticateDefaultCommandHandler : IRequestHandler<PlayerAuthenticateDefaultCommand, CommandResult>
    {
        private readonly ILogger _logger;
        private readonly IMediator _mediator;
        private readonly UserManager<ServerIdentity> _userManager;
        private readonly SignInManager<ServerIdentity> _signInManager;
        private readonly LockoutOptions _lockoutOptions;

        public PlayerAuthenticateDefaultCommandHandler(
            ILogger<PlayerAuthenticateDefaultCommandHandler> logger,
            IMediator mediator,
            UserManager<ServerIdentity> userManager,
            SignInManager<ServerIdentity> signInManager,
            IOptions<LockoutOptions> lockoutOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _lockoutOptions = lockoutOptions.Value ?? throw new ArgumentNullException(nameof(lockoutOptions));
        }

        public async Task<CommandResult> Handle(PlayerAuthenticateDefaultCommand request, CancellationToken ct)
        {
            var (player, password) = request;

            if (await _userManager.FindByNameAsync(player.Name) is not { } user)
            {
                user = new ServerIdentity { Id = ObjectId.NewObjectId(), UserName = player.Name };
                var createResult = await _userManager.CreateAsync(user, password);
                if (createResult is not { Succeeded: true })
                {
                    foreach (var error in createResult.Errors)
                    {
                        await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, error.Description), ct);
                    }

                    return new CommandResult(false);
                }
            }

            var checkResult = await _signInManager.CheckPasswordSignInAsync(user, password, true);
            if (checkResult is not { Succeeded: true })
            {
                if (checkResult.IsLockedOut)
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user) ?? DateTimeOffset.Now;
                    var duration = lockoutEnd - DateTimeOffset.Now;

                    await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, $"You are locked out for {duration.Seconds} seconds!"), ct);
                    return new CommandResult(false);
                }

                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                var attemptsLeft = _lockoutOptions.MaxFailedAccessAttempts - failedCount;

                await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, "Invalid Password!"), ct);
                await _mediator.Publish(new MessageToPlayerNotification(IPlayer.Server, player, $"You have {attemptsLeft} attempts left before lockout!"), ct);
                return new CommandResult(false);
            }

            var claims = await _userManager.GetClaimsAsync(user);
            var permissionClaim = claims.FirstOrDefault(x => x.Type.Equals("server:permissions", StringComparison.OrdinalIgnoreCase));
            var permissions = Enum.TryParse<PermissionFlags>(permissionClaim?.Value, out var permFlags) ? permFlags : PermissionFlags.User;
            await player.AssignPermissionsAsync(permissions, ct);
            return new CommandResult(true);

        }
    }
}