using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Domain;
using P3D.Legacy.Server.Domain.Queries;
using P3D.Legacy.Server.Domain.Queries.Permission;
using P3D.Legacy.Server.Domain.Repositories;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Permission;

internal sealed class PermissionQueryHandler : IQueryHandler<GetPlayerPermissionQuery, PermissionViewModel?>
{
    private readonly ILogger _logger;
    private readonly IPermissionRepository _permissionRepository;

    public PermissionQueryHandler(ILogger<PermissionQueryHandler> logger, IPermissionRepository permissionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
    }

    public async Task<PermissionViewModel?> HandleAsync(GetPlayerPermissionQuery query, CancellationToken ct)
    {
        var id = query.Id;

        return await _permissionRepository.GetByIdAsync(id, ct) is { } entity
            ? new PermissionViewModel(entity.Permissions)
            : new PermissionViewModel(PermissionTypes.UnVerified);
    }
}