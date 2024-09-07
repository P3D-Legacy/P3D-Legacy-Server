using FluentValidation;

using P3D.Legacy.Server.CQERS.Behaviours.Command;
using P3D.Legacy.Server.CQERS.Behaviours.Query;
using P3D.Legacy.Server.CQERS.Commands;
using P3D.Legacy.Server.CQERS.Queries;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Behaviours;

internal class CommandValidationBehaviour<TCommand> : ICommandBehavior<TCommand> where TCommand : ICommand
{
    private readonly IEnumerable<IValidator<TCommand>> _validators;

    public CommandValidationBehaviour(IEnumerable<IValidator<TCommand>> validators)
    {
        _validators = validators;
    }

    public async Task<CommandResult> HandleAsync(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TCommand>(command);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
            var failures = validationResults.SelectMany(static r => r.Errors).Where(static f => f is not null).ToImmutableArray();

            if (failures.Any())
                throw new ValidationException(failures);
        }
        return await next();
    }
}

internal class QueryValidationBehaviour<TQuery, TQueryResult> : IQueryBehavior<TQuery, TQueryResult> where TQuery : IQuery<TQueryResult>
{
    private readonly IEnumerable<IValidator<TQuery>> _validators;

    public QueryValidationBehaviour(IEnumerable<IValidator<TQuery>> validators)
    {
        _validators = validators;
    }

    public async Task<TQueryResult> HandleAsync(TQuery query, QueryHandlerDelegate<TQueryResult> next, CancellationToken ct)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TQuery>(query);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));
            var failures = validationResults.SelectMany(static r => r.Errors).Where(static f => f is not null).ToImmutableArray();

            if (failures.Any())
                throw new ValidationException(failures);
        }
        return await next();
    }
}