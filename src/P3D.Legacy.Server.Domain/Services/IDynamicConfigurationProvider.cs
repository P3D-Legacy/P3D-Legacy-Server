﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace P3D.Legacy.Server.Domain.Services;

public interface IDynamicConfigurationProvider
{
    public Type OptionsType { get; }

    public IEnumerable<PropertyInfo> AvailableProperties { get; }

    public bool SetProperty(PropertyInfo propertyInfo, string value);
}