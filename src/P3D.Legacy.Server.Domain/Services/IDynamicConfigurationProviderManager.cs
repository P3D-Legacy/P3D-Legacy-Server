using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace P3D.Legacy.Server.Domain.Services;

public interface IDynamicConfigurationProviderManager
{
    IEnumerable<Type> GetRegisteredOptionTypes();
    IDynamicConfigurationProvider? GetProvider(Type optionsType);
    IDynamicConfigurationProvider? GetProvider<TOptions>();
    object? GetOptions([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.PublicConstructors)] Type type);
    TOptions GetOptionsInternal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods)] TOptions>() where TOptions : class;
}