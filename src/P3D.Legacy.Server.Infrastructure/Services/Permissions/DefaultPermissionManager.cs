using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;
using P3D.Legacy.Server.Infrastructure.Repositories.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Permissions
{
    public class DefaultPermissionManager : IPermissionManager
    {
        private readonly ServerOptions _options;
        private readonly LiteDbPermissionRepository _liteDbPermissionRepository;
        private readonly P3DPermissionRepository _p3dPermissionRepository;

        public DefaultPermissionManager(IOptions<ServerOptions> options, LiteDbPermissionRepository liteDbPermissionRepository, P3DPermissionRepository p3dPermissionRepository)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _liteDbPermissionRepository = liteDbPermissionRepository ?? throw new ArgumentNullException(nameof(liteDbPermissionRepository));
            _p3dPermissionRepository = p3dPermissionRepository ?? throw new ArgumentNullException(nameof(p3dPermissionRepository));
        }

        public async Task<PermissionEntity> GetByIdAsync(PlayerId id, CancellationToken ct) => id.IdType switch
        {
            PlayerIdType.Name => await _liteDbPermissionRepository.GetByNameIdAsync(id.Id, ct),
            PlayerIdType.GameJolt => _options.IsOfficial
                ? await _p3dPermissionRepository.GetByGameJoltIdAsync(GameJoltId.Parse(id.Id), ct)
                : await _liteDbPermissionRepository.GetByGameJoltIdAsync(GameJoltId.Parse(id.Id), ct),
            _ => new PermissionEntity(PermissionFlags.UnVerified)
        };

        public async Task<bool> SetPermissionsAsync(PlayerId id, PermissionFlags permissions, CancellationToken ct) => id.IdType switch
        {
            PlayerIdType.Name => await _liteDbPermissionRepository.UpdateAsync(id, permissions, ct),
            PlayerIdType.GameJolt => !_options.IsOfficial && await _liteDbPermissionRepository.UpdateAsync(id, permissions, ct),
            _ => false
        };
    }
}