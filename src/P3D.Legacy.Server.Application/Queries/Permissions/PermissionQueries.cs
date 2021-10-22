using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Permissions
{
    public class PermissionQueries : IPermissionQueries
    {
        private readonly ILogger _logger;
        private readonly IPermissionRepository _permissionRepository;

        public PermissionQueries(ILogger<PermissionQueries> logger, IPermissionRepository permissionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        }

        public async Task<PermissionViewModel> GetByGameJoltAsync(ulong id, CancellationToken ct) => await _permissionRepository.GetByGameJoltAsync(id, ct) is { } entity
            ? new PermissionViewModel(entity.Permissions)
            : new PermissionViewModel(PermissionFlags.UnVerified);
    }
}