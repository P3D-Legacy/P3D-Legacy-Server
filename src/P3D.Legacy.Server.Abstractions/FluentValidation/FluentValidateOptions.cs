using FluentValidation;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
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
            var failures = new List<string>();
            foreach (var validator in _validators)
            {
                var result = validator.Validate(options);
                failures.AddRange(result.Errors.Select(x => x.ErrorMessage));
            }

            return failures.Any() ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
        }
    }
}
