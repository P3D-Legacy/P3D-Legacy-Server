using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework;

public class ServerHostedService(IOptions<ServerHostedServiceOptions> options) : IHostedService
{
    private readonly Server _server = options.Value.ServerBuilder.Build();

    public Task StartAsync(CancellationToken ct) => _server.StartAsync(ct);

    public Task StopAsync(CancellationToken ct) => _server.StopAsync(ct);
}