using Microsoft.Extensions.Logging;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Queries.Permissions
{
    public class PermissionQueries : IPermissionQueries
    {
        private readonly ILogger _logger;
        private readonly IPermissionManager _permissionRepository;

        public PermissionQueries(ILogger<PermissionQueries> logger, IPermissionManager permissionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
        }

        public async Task<PermissionViewModel> GetByIdAsync(PlayerId id, CancellationToken ct) => await _permissionRepository.GetByIdAsync(id, ct) is { } entity
            ? new PermissionViewModel(entity.Permissions)
            : new PermissionViewModel(PermissionFlags.UnVerified);
    }
}