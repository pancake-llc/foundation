using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

namespace Pancake.Init
{
    using static NullExtensions;

    internal class ConversionExtensions
    {
        internal static bool TryConvert<T>([NotNull] object obj, out T result)
        {
            if(obj is T castable)
            {
                if(castable != Null)
                {
                    result = castable;
                    return true;
                }

                result = default;
                return false;
            }

            if(obj is IValueProvider<T> provider)
            {
                var value = provider.Value;
                if(value != Null)
                {
                    result = value;
                    return true;
                }

                result = default;
                return false;
            }

            if(obj is IValueProvider objectProvider && objectProvider.Value is T valueAsService)
            {
                if(valueAsService != Null)
                {
                    result = valueAsService;
                    return true;
                }

                result = default;
                return false;
            }

            var converter = TypeDescriptor.GetConverter(obj.GetType());
            if(converter != null && converter.CanConvertTo(typeof(T)))
            {
                object converted = converter.ConvertTo(obj, typeof(T));
                if(converted != null)
                {
                    result = (T)converted;
                    return true;
                }
            }

            result = default;
            return false;
        }

        internal static T As<T>([NotNull] Object obj)
        {
            if(obj is T result)
            {
                return result;
            }

            if(obj is IValueProvider<T> providerOfT)
            {
                return providerOfT.Value;
            }
            if(obj is IValueProvider provider && provider.Value is object value && typeof(T).IsAssignableFrom(value.GetType()))
            {
                return (T)value;
            }

            return default;
        }

        internal static object As([NotNull] Type type, [NotNull] Object obj)
        {
            if(type.IsAssignableFrom(obj.GetType()))
            {
                return obj;
            }

            if(obj is IWrapper wrapper && wrapper.WrappedObject is object wrapped && type.IsAssignableFrom(wrapped.GetType()))
            {
                return wrapper.WrappedObject;
            }

            if(obj is IValueProvider provider && provider.Value is object value && type.IsAssignableFrom(value.GetType()))
            {
                return provider.Value;
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        internal static void TryAddAs<T>([NotNull] List<T> list, [NotNull] Object obj)
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

        internal static void TryAddAs([NotNull] List<object> list, [NotNull] Object obj, [NotNull] Type asType)
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

        internal static void TryAddAs<T>([NotNull] List<T> list, [NotNull] Object[] objs)
        {
            for(int i = objs.Length - 1; i >= 0; i--)
            {
                TryAddAs(list, objs[i]);
            }
        }
    }
}