using FluentValidation;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public class FluentValidateOptions<TOptions> : IValidateOptions<TOptions> where TOptions : class
    {
        private readonly IEnumerable<IValidator<TOptions>> _validators;

        public FluentValidateOptions(IEnumerable<IValidator<TOptions>> validators)
        {
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public ValidateOptionsResult Validate(string name, TOptions options)
        {
            static IEnumerable<string> GetFailures(IEnumerable<IValidator<TOptions>> validators, string name, TOptions options)
            {
                foreach (var validator in validators)
                {
                    var result = validator.Validate(options);
                    foreach (var failure in result.Errors.Select(x => x.ErrorMessage))
                    {
                        yield return failure;
                    }
                }
            }

            var failures = GetFailures(_validators, name, options).ToImmutableArray();
            return failures.Any() ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
        }
    }
}
