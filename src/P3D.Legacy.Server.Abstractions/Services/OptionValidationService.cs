using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Options;

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Abstractions.Services
{
    public sealed class OptionValidationService : IHostedService
    {
        private readonly ValidatorOptions _validatorOptions;

        public OptionValidationService(IOptions<ValidatorOptions> validatorOptions)
        {
            _validatorOptions = validatorOptions.Value ?? throw new ArgumentNullException(nameof(validatorOptions));
        }

        public Task StartAsync(CancellationToken ct)
        {
            var exceptions = new List<Exception>();

            foreach (var validate in _validatorOptions.Validators.Values)
            {
                try
                {
                    // Execute the validation method and catch the validation error
                    validate();
                }
                catch (OptionsValidationException ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count == 1)
            {
                // Rethrow if it's a single error
                ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
            }

            if (exceptions.Count > 1)
            {
                // Aggregate if we have many errors
                throw new AggregateException(exceptions);
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
