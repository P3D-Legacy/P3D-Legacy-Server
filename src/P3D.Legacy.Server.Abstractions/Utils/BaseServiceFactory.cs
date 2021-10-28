using System;

namespace P3D.Legacy.Server.Abstractions.Utils
{
    internal abstract class BaseServiceFactory
    {
        public abstract object ServiceFactory(IServiceProvider sp);
    }
}