using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Open.Nat;

using P3D.Legacy.Server.Client.P3D.Options;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Services
{
    public sealed class P3DNATHandler : IHostedService
    {
        private readonly ILogger _logger;
        private readonly P3DServerOptions _options;

        public P3DNATHandler(ILogger<P3DNATHandler> logger, IOptions<P3DServerOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken ct)
        {
            if (!_options.PortForward)
            {
                _logger.LogWarning("Port Forwarding is disabled");
                return;
            }

            _logger.LogWarning("Port Forwarding is enabled. Forwarding P3D port {P3DPort} with timeout {P3DPortForwardTimeoutMilliseconds}", _options.Port, _options.PortForwardTimeoutMilliseconds);
            var discoverer = new NatDiscoverer();
            using var cts = new CancellationTokenSource(_options.PortForwardTimeoutMilliseconds);
            using var combined = CancellationTokenSource.CreateLinkedTokenSource(ct, cts.Token);
            await await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, combined).ContinueWith(async result =>
            {
                if (result.IsFaulted)
                {
                    _logger.LogWarning("Failed to forward port! UPNP device not found!");
                    return;
                }

                var device = await result;
                var ip = await device.GetExternalIPAsync();
                _logger.LogWarning("The public IP Address for P3D is: {PublicIPAddres}", ip);

                await device.CreatePortMapAsync(new Mapping(Open.Nat.Protocol.Tcp, _options.Port, _options.Port, "P3D Server Port"));
                _logger.LogWarning("Forwarded port {P3DPort}", _options.Port);
            }, CancellationToken.None, TaskContinuationOptions.None, TaskScheduler.Current);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}