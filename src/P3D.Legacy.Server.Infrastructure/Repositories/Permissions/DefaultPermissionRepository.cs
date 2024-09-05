using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Permissions;

internal class DefaultPermissionRepository : IPermissionRepository
{
    private readonly P3DIntegrationOptions _options;
    private readonly LiteDbPermissionRepository _liteDbPermissionRepository;
    private readonly P3DPermissionRepository _p3dPermissionRepository;

    public DefaultPermissionRepository(IOptions<P3DIntegrationOptions> options, LiteDbPermissionRepository liteDbPermissionRepository, P3DPermissionRepository p3dPermissionRepository)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _liteDbPermissionRepository = liteDbPermissionRepository ?? throw new ArgumentNullException(nameof(liteDbPermissionRepository));
        _p3dPermissionRepository = p3dPermissionRepository ?? throw new ArgumentNullException(nameof(p3dPermissionRepository));
    }

    public async Task<PermissionEntity> GetByIdAsync(PlayerId id, CancellationToken ct) => id.IdType switch
    {
        PlayerIdType.Name => await _liteDbPermissionRepository.GetByNameIdAsync(id.NameOrEmpty, ct),
        PlayerIdType.GameJolt => _options.IsOfficial
            ? await _p3dPermissionRepository.GetByGameJoltIdAsync(id.GameJoltIdOrNone, ct)
            : await _liteDbPermissionRepository.GetByGameJoltIdAsync(id.GameJoltIdOrNone, ct),
        _ => new PermissionEntity(PermissionTypes.UnVerified)
    };

    public async Task<bool> SetPermissionsAsync(PlayerId id, PermissionTypes permissions, CancellationToken ct) => id.IdType switch
    {
        PlayerIdType.Name => await _liteDbPermissionRepository.UpdateAsync(id, permissions, ct),
        PlayerIdType.GameJolt => !_options.IsOfficial && await _liteDbPermissionRepository.UpdateAsync(id, permissions, ct),
        _ => false
    };
}