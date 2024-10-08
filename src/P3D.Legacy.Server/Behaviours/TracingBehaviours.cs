﻿using OpenTelemetry.Trace;

using P3D.Legacy.Server.CQERS.Behaviours.Command;
using P3D.Legacy.Server.CQERS.Behaviours.Query;
using P3D.Legacy.Server.Domain.Commands;
using P3D.Legacy.Server.Domain.Queries;

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours;

internal class CommandTracingBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
{
    private readonly Tracer _tracer;

    public int Order => 2000;

    public CommandTracingBehaviour(TracerProvider tracerProvider)
    {
        _tracer = tracerProvider.GetTracer("P3D.Legacy.Server.Host");
    }

    public async Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        var span = _tracer.StartActiveSpan($"{typeof(TCommand).Name} Handle");
        try
        {
            return await next();
        }
        catch
        {
            span.SetStatus(Status.Error);
            throw;
        }
        finally
        {
            span.Dispose();
        }
    }
}

internal class QueryTracingBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    private readonly Tracer _tracer;

    public QueryTracingBehaviour(TracerProvider tracerProvider)
    {
        _tracer = tracerProvider.GetTracer("P3D.Legacy.Server.Host");
    }

    public int Order => 2000;

    public async Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        var span = _tracer.StartActiveSpan($"{typeof(TQuery).Name} Handle");
        try
        {
            return await next();
        }
        catch
        {
            span.SetStatus(Status.Error);
            throw;
        }
        finally
        {
            span.Dispose();
        }
    }
}