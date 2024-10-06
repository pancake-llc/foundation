using System;
using System.Diagnostics.CodeAnalysis;
using Object = UnityEngine.Object;

namespace Sisus.Init.EditorOnly.Internal
{
    internal static class WrapperUtility
    {
        internal static bool TryGetGetWrappedObjectType([DisallowNull] Object wrapper, [MaybeNullWhen(false), NotNullWhen(true)] out Type result)
        {
            var wrapperType = wrapper.GetType();
            if(TryGetGetWrappedObjectType(wrapperType, out result))
            {
                return true;
            }

            if(wrapper is IWrapper { WrappedObject: object wrappedObject } )
            {
                result = wrappedObject.GetType();
                return true;
            }

            return false;
        }

        internal static bool TryGetGetWrappedObjectType([DisallowNull] Type wrapperType, [MaybeNullWhen(false), NotNullWhen(true)] out Type result)
        {
            foreach(var interfaceType in wrapperType.GetInterfaces())
            {
                if(!interfaceType.IsGenericType)
                {
                    continue;
                }

                // Get interfaces -> find -> Wrapper<> -> Get generic type Argument

                var genericTypeDefinition = interfaceType.IsGenericTypeDefinition ? interfaceType : interfaceType.GetGenericTypeDefinition();
                if(genericTypeDefinition == typeof(IWrapper<>))
                {
                    result = interfaceType.GetGenericArguments()[0];
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}