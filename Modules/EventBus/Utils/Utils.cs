using System;
using System.Runtime.CompilerServices;

namespace Pancake.EventBus.Utils
{
    internal static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Implements<T>(this Type source) where T : class
        {
            return typeof(T).IsAssignableFrom(source);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsNull<T>(this T o) where T : class
        {
            return ReferenceEquals(o, null);
        }
    }
}