using LiteDB;

using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Permissions
{
    public class LiteDbPermissionRepository : IPermissionRepository
    {
        private record Permission(ulong GameJoltId, PermissionFlags Permissions);

        private readonly LiteDbOptions _options;

        public LiteDbPermissionRepository(IOptions<LiteDbOptions> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Task<PermissionEntity> GetByGameJoltAsync(GameJoltId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabase(_options.Path);
            var permissions = db.GetCollection<Permission>("permissions");

            var result = permissions.FindOne(x => x.GameJoltId == id);
            return Task.FromResult(new PermissionEntity(result?.Permissions ?? PermissionFlags.UnVerified));
        }
    }
}