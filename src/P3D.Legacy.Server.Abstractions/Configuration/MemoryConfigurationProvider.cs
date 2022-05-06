using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace P3D.Legacy.Server.Abstractions.Configuration
{
    public class MemoryConfigurationProvider<TOptions> : ConfigurationProvider, IConfigurationSource, IDynamicConfigurationProvider
    {
        public Type OptionsType => typeof(TOptions);
        public IEnumerable<PropertyInfo> AvailableProperties => _keys;

        private readonly string _basePath;
        private readonly PropertyInfo[] _keys;
        public MemoryConfigurationProvider(IConfigurationSection section)
        {
            _basePath = section.Path;
            _keys = typeof(TOptions).GetProperties().Where(static p => p.CanRead && p.CanWrite).ToArray();
        }

        public bool SetProperty(PropertyInfo propertyInfo, string value)
        {
            Set($"{_basePath}:{propertyInfo.Name}", value);
            OnReload();
            return true;
        }

        public override void Load() { }

        public IConfigurationProvider Build(IConfigurationBuilder builder) => this;
    }
}