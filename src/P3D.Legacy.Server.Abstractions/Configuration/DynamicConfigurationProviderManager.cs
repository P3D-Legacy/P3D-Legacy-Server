using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3D.Legacy.Server.Abstractions.Configuration
{
    public class DynamicConfigurationProviderManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDynamicConfigurationProvider[] _configurationProviders = Array.Empty<IDynamicConfigurationProvider>();

        public DynamicConfigurationProviderManager(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if (configuration is IConfigurationRoot root)
            {
                _configurationProviders = root.Providers.OfType<IDynamicConfigurationProvider>().ToArray();
            }
        }

        public IEnumerable<Type> GetRegisteredOptionTypes() => _configurationProviders.Select(static x => x.OptionsType);

        public IDynamicConfigurationProvider? GetProvider(Type optionsType) => _configurationProviders.FirstOrDefault(x => x.OptionsType == optionsType);
        public IDynamicConfigurationProvider? GetProvider<TOptions>() => GetProvider(typeof(TOptions));
        public object? GetOptions(Type type)
        {
            var openMethod = typeof(DynamicConfigurationProviderManager).GetMethod(nameof(GetOptionsInternal), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            var method = openMethod?.MakeGenericMethod(type);
            return method?.Invoke(this, parameters: null);
        }
        public TOptions? GetOptionsInternal<TOptions>() where TOptions : class => _serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
    }
}