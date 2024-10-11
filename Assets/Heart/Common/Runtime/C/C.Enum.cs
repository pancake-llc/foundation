using System;

namespace Pancake.Common
{
    public static partial class C
    {
        /// <summary>
        /// Faster and not create garbage allocation
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool HasFlagUnsafe<TEnum>(this TEnum lhs, TEnum rhs) where TEnum : unmanaged, Enum
        {
            unsafe
            {
                switch (sizeof(TEnum))
                {
                    case 1: return (*(byte*) &lhs & *(byte*) &rhs) > 0;
                    case 2: return (*(ushort*) &lhs & *(ushort*) &rhs) > 0;
                    case 4: return (*(uint*) &lhs & *(uint*) &rhs) > 0;
                    case 8: return (*(ulong*) &lhs & *(ulong*) &rhs) > 0;
                    default: throw new Exception("Size does not match a known Enum backing type.");
                }
            }
        }
    }
}