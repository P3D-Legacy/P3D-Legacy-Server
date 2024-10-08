﻿using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Commands.Administration;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.CommandHandlers.Administration;

internal sealed class UnbanPlayerCommandHandler : ICommandHandler<UnbanPlayerCommand>
{
    private readonly ILogger _logger;
    private readonly IBanRepository _banRepository;

    public UnbanPlayerCommandHandler(ILogger<UnbanPlayerCommandHandler> logger, IBanRepository banRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _banRepository = banRepository ?? throw new ArgumentNullException(nameof(banRepository));
    }

    public async Task<CommandResult> HandleAsync(UnbanPlayerCommand command, CancellationToken ct)
    {
        var result = await _banRepository.UnbanAsync(command.Id, ct);

        return new CommandResult(result);
    }
}