using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Abstractions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

namespace P3D.Legacy.Server.Abstractions.Extensions
{
    public static class HostExtensions
    {
        public static IHost ValidateOptions(this IHost host)
        {
            var options = host.Services.GetRequiredService<IOptions<ValidatorOptions>>();

            var exceptions = new List<Exception>();

            foreach (var validate in options.Value?.Validators.Values ?? Enumerable.Empty<Action>())
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

            return host;
        }
    }
}
