using System;
using System.Collections.Generic;

namespace P3D.Legacy.Server.Abstractions.Options
{
    internal sealed record ValidatorOptions
    {
        // Maps each options type to a method that forces its evaluation, e.g. IOptionsMonitor<TOptions>.Get(name)
        public IDictionary<Type, Action> Validators { get; } = new Dictionary<Type, Action>();
    }
}