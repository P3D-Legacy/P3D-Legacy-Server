using Bedrock.Framework.Protocols;

using Microsoft.AspNetCore.Connections.Features;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using OpenTelemetry.Trace;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Packets.Client;
using P3D.Legacy.Server.Abstractions;
using P3D.Legacy.Server.Application.Commands.Player;
using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Events;
using P3D.Legacy.Server.CQERS.Queries;

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D
{
    internal sealed partial class P3DConnectionContextHandler : ConnectionContextHandler, IPlayer
    {
        private readonly ILogger _logger;
        private readonly Tracer _tracer;
        private readonly P3DProtocol _protocol;

        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly IEventDispatcher _eventDispatcher;

        private TelemetrySpan _connectionSpan = default!;
        private ProtocolWriter _writer = default!;

        public P3DConnectionContextHandler(
            ILogger<P3DConnectionContextHandler> logger,
            TracerProvider traceProvider,
            P3DProtocol protocol,
            ICommandDispatcher commandDispatcher,
            IQueryDispatcher queryDispatcher,
            IEventDispatcher eventDispatcher)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tracer = traceProvider.GetTracer("P3D.Legacy.Server.Client.P3D");
            _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
            _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
            _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        protected override async Task OnCreatedAsync(CancellationToken ct)
        {
            _connectionSpan = _tracer.StartActiveSpan("P3D Session UNKNOWN");

            try
            {
                if (Connection.RemoteEndPoint is IPEndPoint ipEndPoint)
                {
                    IPEndPoint = ipEndPoint;
                }

                Connection.Features.Get<IConnectionLifetimeNotificationFeature>()?.ConnectionClosedRequested.Register(() =>
                {
                    using var finishSpan = _tracer.StartActiveSpan("P3D Client Closing", parentContext: _connectionSpan.Context);
                    var oldState = State;
                    State = PlayerState.Finalizing;
                    if (oldState == PlayerState.Initialized)
                    {
                        using var jctx = new JoinableTaskContext(); // Should I create it here or in the main method?
                        new JoinableTaskFactory(jctx).Run(() => _commandDispatcher.DispatchAsync(new PlayerFinalizingCommand(this), CancellationToken.None));
                    }
                    State = PlayerState.Finalized;
                });

                _writer = Connection.CreateWriter();
                await using var _ = _writer;

                await using var reader = Connection.CreateReader();
                var watch = Stopwatch.StartNew();
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (await reader.ReadAsync(_protocol, ct) is { Message: { } message, IsCompleted: var isCompleted, IsCanceled: var isCanceled })
                        {
                            using var span = _tracer.StartActiveSpan($"P3D Client Reading {message.GetType().FullName}", SpanKind.Server);
                            span.SetAttribute("net.peer.ip", IPEndPoint.Address.ToString());
                            span.SetAttribute("net.peer.port", IPEndPoint.Port);
                            span.SetAttribute("net.transport", "ip_tcp");
                            span.SetAttribute("p3dclient.packet_type", message.GetType().FullName);

                            await HandlePacketAsync(message, ct);

                            if (isCompleted || isCanceled)
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        reader.Advance();
                    }

                    if (State == PlayerState.Initialized && watch.ElapsedMilliseconds >= 5000)
                    {
                        await SendPacketAsync(new PingPacket { Origin = Origin.Server }, ct);
                        watch.Restart();
                    }
                }
            }
            catch (Exception e)
            {
                _connectionSpan.RecordException(e).SetStatus(Status.Error);
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                State = PlayerState.None;
                _connectionSpan.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}