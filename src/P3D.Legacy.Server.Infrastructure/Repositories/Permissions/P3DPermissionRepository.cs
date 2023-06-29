using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Permissions;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Permissions
{
    // TODO: public
    internal class P3DPermissionRepository
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly Pokemon3DAPIClient _apiClient;

        public P3DPermissionRepository(ILogger<P3DPermissionRepository> logger, TracerProvider traceProvider, Pokemon3DAPIClient apiClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Infrastructure");
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        }

        public async Task<PermissionEntity> GetByGameJoltIdAsync(GameJoltId id, CancellationToken ct)
        {
            using var span = _tracer.StartActiveSpan("P3DPermissionRepository GetByGameJoltAsync", SpanKind.Client);

            return await _apiClient.GetByGameJoltIdAsync(id, ct);
        }
    }
}