using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

using System.Net;

namespace P3D.Legacy.Server.Client.P3D.Options;

public class P3DServerOptionsSetup : IPostConfigureOptions<KestrelServerOptions>
{
    private readonly P3DServerOptions _options;

    public P3DServerOptionsSetup(IOptions<P3DServerOptions> options)
    {
        _options = options.Value;
    }

    public void PostConfigure(string? name, KestrelServerOptions options)
    {
        options.Listen(new IPEndPoint(IPAddress.Parse(_options.IP), _options.Port), static builder =>
        {
            builder.UseConnectionLogging().UseConnectionHandler<P3DConnectionHandler>();
        });
    }
}