using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sisus.Init.ValueProviders;
using Object = UnityEngine.Object;

namespace Sisus.Init
{
    using static NullExtensions;

    internal static class ConversionExtensions
    {
        internal static bool TryConvert<T>([DisallowNull] object obj, [NotNullWhen(true), MaybeNullWhen(false)] out T result)
        {
            if(obj is T castable)
            {
                result = castable;
                return result != Null;
            }

            return ValueProviderUtility.TryGetValueProviderValue(obj, out result);
        }

        internal static T As<T>([DisallowNull] Object obj)
        {
            if(obj is T result)
            {
                return result;
            }

            ValueProviderUtility.TryGetValueProviderValue(obj, out result);
            return result;
        }

        internal static object As([DisallowNull] Type type, [DisallowNull] Object obj)
        {
            if(type.IsInstanceOfType(obj))
            {
                return obj;
            }

            if(ValueProviderUtility.TryGetValueProviderValue(obj, type, out var result))
            {
                return result;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        internal static void TryAddAs<T>([DisallowNull] List<T> list, [DisallowNull] Object obj)
        {
            if(obj is T result)
            {
                list.Add(result);
                return;
            }

            if(obj is IWrapper wrapper)
            {
                list.Add((T)wrapper.WrappedObject);
            }
        }

        internal static void TryAddAs([DisallowNull] List<object> list, [DisallowNull] Object obj, [DisallowNull] Type asType)
        {
            var objectType = obj.GetType();
            if(asType.IsAssignableFrom(objectType))
            {
                list.Add(obj);
                return;
            }

            if(obj is IWrapper wrapper)
            {
                list.Add(wrapper.WrappedObject);
            }
        }

        internal static void TryAddAs<T>([DisallowNull] List<T> list, [DisallowNull] Object[] objs)
        {
            int count = objs.Length;
            if(list.Capacity < count)
            {
                list.Capacity = count;
            }

            for(int i = count - 1; i >= 0; i--)
            {
                TryAddAs(list, objs[i]);
            }
        }
    }
}