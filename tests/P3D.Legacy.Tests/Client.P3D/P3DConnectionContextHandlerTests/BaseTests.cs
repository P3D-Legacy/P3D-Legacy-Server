using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Trace;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Client.P3D;
using P3D.Legacy.Server.Client.P3D.Packets;
using P3D.Legacy.Tests.Utils;

using System;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Client.P3D.P3DConnectionContextHandlerTests;

internal abstract class BaseTests
{
    private class TestConnectionFactory : IConnectionFactory
    {
        private class DuplexPipe : IDuplexPipe
        {
            // This class exists to work around issues with value tuple on .NET Framework
            public readonly struct DuplexPipePair
            {
                public IDuplexPipe Transport { get; }
                public IDuplexPipe Application { get; }

                public DuplexPipePair(IDuplexPipe transport, IDuplexPipe application)
                {
                    Transport = transport;
                    Application = application;
                }
            }

            public static DuplexPipePair CreateConnectionPair(PipeOptions inputOptions, PipeOptions outputOptions)
            {
                var input = new Pipe(inputOptions);
                var output = new Pipe(outputOptions);

                var transportToApplication = new DuplexPipe(output.Reader, input.Writer);
                var applicationToTransport = new DuplexPipe(input.Reader, output.Writer);

                return new DuplexPipePair(applicationToTransport, transportToApplication);
            }

            public PipeReader Input { get; }
            public PipeWriter Output { get; }

            private DuplexPipe(PipeReader reader, PipeWriter writer)
            {
                Input = reader;
                Output = writer;
            }
        }
        private class TestConnectionLifetimeNotificationFeature : IConnectionLifetimeNotificationFeature
        {
            public CancellationToken ConnectionClosedRequested { get; set; }

            private readonly ConnectionContext _connectionContext;

            public TestConnectionLifetimeNotificationFeature(ConnectionContext connectionContext)
            {
                _connectionContext = connectionContext;
                ConnectionClosedRequested = connectionContext.ConnectionClosed;
            }
            public void RequestClose() => _connectionContext.Abort();
        }

        public ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, CancellationToken ct = default)
        {
            var options = new PipeOptions(useSynchronizationContext: false);
            var pair = DuplexPipe.CreateConnectionPair(options, options);
            var connectionContext = new DefaultConnectionContext(Guid.NewGuid().ToString(), pair.Transport, pair.Application);
            connectionContext.Features.Set<IConnectionLifetimeNotificationFeature>(new TestConnectionLifetimeNotificationFeature(connectionContext));
            return ValueTask.FromResult<ConnectionContext>(connectionContext);
        }
    }

    protected static TestService Common() => TestService.CreateNew().Configure(static services =>
    {
        services.AddSingleton<TracerProvider>(static _ => TracerProvider.Default);

        services.AddSingleton<IConnectionFactory, TestConnectionFactory>();

        services.AddScoped<ConnectionContextHandlerFactory>();

        services.AddTransient<P3DConnectionHandler>();
        services.AddScoped<P3DConnectionContextHandler>();

        services.AddScoped<P3DProtocol>();
        services.AddSingleton<IP3DPacketFactory, P3DPacketServerFactory>();
    });
}