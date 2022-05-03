using Microsoft.Extensions.Logging;

using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Queries.Permission;
using P3D.Legacy.Server.CQERS.Queries;
using P3D.Legacy.Server.Infrastructure.Services.Permissions;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.QueryHandlers.Permission
{
    [SuppressMessage("Performance", "CA1812")]
    internal sealed class PermissionQueryHandler : IQueryHandler<GetPlayerPermissionQuery, PermissionViewModel?>
    {
        private readonly ILogger _logger;
        private readonly IPermissionManager _permissionRepository;

        public PermissionQueryHandler(ILogger<PermissionQueryHandler> logger, IPermissionManager permissionRepository)
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
}