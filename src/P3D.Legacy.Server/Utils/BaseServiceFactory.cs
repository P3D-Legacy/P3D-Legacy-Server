using System;

namespace P3D.Legacy.Server.Utils
{
    internal abstract class BaseServiceFactory
    {
        public abstract object ServiceFactory(IServiceProvider sp);
    }
}