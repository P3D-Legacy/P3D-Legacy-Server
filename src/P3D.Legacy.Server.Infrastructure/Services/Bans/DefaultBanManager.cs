using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Abstractions.Options;
using P3D.Legacy.Server.Infrastructure.Models.Bans;
using P3D.Legacy.Server.Infrastructure.Repositories.Bans;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Bans
{
    public sealed class DefaultBanManager : IBanManager
    {
        private readonly ServerOptions _options;
        private readonly LiteDbBanRepository _liteDbBanRepository;
        private readonly P3DBanRepository _p3dBanRepository;

        public DefaultBanManager(IOptions<ServerOptions> options, LiteDbBanRepository liteDbBanRepository, P3DBanRepository p3dBanRepository)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _liteDbBanRepository = liteDbBanRepository ?? throw new ArgumentNullException(nameof(liteDbBanRepository));
            _p3dBanRepository = p3dBanRepository ?? throw new ArgumentNullException(nameof(p3dBanRepository));
        }

        public async Task<BanEntity?> GetAsync(PlayerId id, CancellationToken ct) => id.IdType switch
        {
            PlayerIdType.Name => await _liteDbBanRepository.GetAsync(id, ct),
            PlayerIdType.GameJolt => _options.IsOfficial ? await _p3dBanRepository.GetAsync(id, ct) : await _liteDbBanRepository.GetAsync(id, ct),
            _ => null
        };

        public async IAsyncEnumerable<BanEntity> GetAllAsync([EnumeratorCancellation] CancellationToken ct)
        {
            if (_options.IsOfficial)
            {
                await foreach (var banEntity in _p3dBanRepository.GetAllAsync(ct))
                {
                    yield return banEntity;
                }
            }

            await foreach (var banEntity in _p3dBanRepository.GetAllAsync(ct))
            {
                yield return banEntity;
            }
        }

        public async Task<bool> BanAsync(BanEntity banEntity, CancellationToken ct) => banEntity.Id.IdType switch
        {
            PlayerIdType.Name => await _liteDbBanRepository.BanAsync(banEntity, ct),
            PlayerIdType.GameJolt => _options.IsOfficial ? await _p3dBanRepository.BanAsync(banEntity, ct) : await _liteDbBanRepository.BanAsync(banEntity, ct),
            _ => false
        };

        public async Task<bool> UnbanAsync(PlayerId id, CancellationToken ct) => id.IdType switch
        {
            PlayerIdType.Name => await _liteDbBanRepository.UnbanAsync(id, ct),
            PlayerIdType.GameJolt => _options.IsOfficial ? await _p3dBanRepository.UnbanAsync(id, ct) : await _liteDbBanRepository.UnbanAsync(id, ct),
            _ => false
        };
    }
}