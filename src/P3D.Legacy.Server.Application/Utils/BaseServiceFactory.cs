using System;

namespace P3D.Legacy.Server.Application.Utils
{
    internal abstract class BaseServiceFactory
    {
        public abstract object ServiceFactory(IServiceProvider sp);
    }
}