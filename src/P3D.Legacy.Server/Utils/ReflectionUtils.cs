using System;
using System.Linq;

namespace P3D.Legacy.Server.Utils
{
    internal static class ReflectionUtils
    {
        public static bool IsAssignableToGenericType(Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
                return true;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;
            return baseType is not null && IsAssignableToGenericType(baseType, genericType);
        }
    }
}
