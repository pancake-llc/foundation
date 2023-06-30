using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Pancake.Tag
{
    // Modified versions of Unity.Mathematics.uint4 and Unity.Entities.Hash128
    // in order to avoid Tag having dependency on the entire Entities & Mathematics packages
    [Serializable]
    public struct Buint4 : IEquatable<Buint4>, IComparable<Buint4>
    {
        public uint x;
        public uint y;
        public uint z;
        public uint w;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Buint4 obj) { return x.Equals(obj.x) && y.Equals(obj.y) && z.Equals(obj.z) && w.Equals(obj.w); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            uint hash = x;
            hash *= 37;
            hash += y;
            hash *= 37;
            hash += z;
            hash *= 37;
            hash += w;
            return (int) hash;
        }

        public int CompareTo(Buint4 other)
        {
            if (w != other.w)
                return w < other.w ? -1 : 1;
            if (z != other.z)
                return z < other.z ? -1 : 1;
            if (y != other.y)
                return y < other.y ? -1 : 1;
            if (x != other.x)
                return x < other.x ? -1 : 1;
            return 0;
        }

        /// <summary>Returns the uint element at a specified index.</summary>
        public unsafe uint this[int index]
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (Buint4* array = &this)
                {
                    return ((uint*) array)[index];
                }
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if ((uint) index >= 4)
                    throw new System.ArgumentException("index must be between[0...3]");
#endif
                fixed (uint* array = &x)
                {
                    array[index] = value;
                }
            }
        }
    }

    public class Hash128EqualityComparer : System.Collections.Generic.EqualityComparer<Hash128>
    {
        public override bool Equals(Hash128 b1, Hash128 b2) { return b1.Equals(b2); }

        public override int GetHashCode(Hash128 hash) { return hash.GetHashCode(); }
    }

    [Serializable]
    public struct Hash128 : IEquatable<Hash128>, IComparable<Hash128>
    {
        [SerializeField] public Buint4 Value;
        public uint x => Value.x;
        public uint y => Value.y;
        public uint z => Value.z;
        public uint w => Value.w;

        private static readonly char[] k_HexToLiteral = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};

        //public BHash128(uint[] value) => Value = value;
        public Hash128(uint x, uint y, uint z, uint w) => Value = new Buint4 {x = x, y = y, z = z, w = w};

        /// <summary>
        /// Construct a hash from a 32 character hex string
        /// If the string has the incorrect length or non-hex characters the Value will be all 0
        /// </summary>
        public unsafe Hash128(string value)
        {
            fixed (char* ptr = value)
            {
                Value = StringToHash(ptr, value.Length);
            }
        }

        public override unsafe string ToString()
        {
            var chars = stackalloc char[32];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    uint cur = Value[i];
                    cur >>= (j * 4);
                    cur &= 0xF;
                    chars[i * 8 + j] = k_HexToLiteral[cur];
                }
            }

            return new string(chars, 0, 32);
        }

        private static readonly sbyte[] k_LiteralToHex =
        {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
        };

        private const int k_GUIDStringLength = 32;

        private static unsafe Buint4 StringToHash(char* guidString, int length)
        {
            if (length != k_GUIDStringLength)
                return default;

            // Convert every hex char into an int [0...16]
            var hex = stackalloc int[k_GUIDStringLength];
            for (int i = 0; i < k_GUIDStringLength; i++)
            {
                int intValue = guidString[i];
                if (intValue < 0 || intValue > 255)
                    return default;

                hex[i] = k_LiteralToHex[intValue];
            }

            Buint4 value = new Buint4();
            for (int i = 0; i < 4; i++)
            {
                uint cur = 0;
                for (int j = 7; j >= 0; j--)
                {
                    int curHex = hex[i * 8 + j];
                    if (curHex == -1)
                        return default;

                    cur |= (uint) (curHex << (j * 4));
                }

                value[i] = cur;
            }

            return value;
        }

        public static bool operator ==(Hash128 obj1, Hash128 obj2)
        {
            return obj1.x.Equals(obj2.x) && obj1.y.Equals(obj2.y) && obj1.z.Equals(obj2.z) && obj1.w.Equals(obj2.w);
        }

        public static bool operator !=(Hash128 obj1, Hash128 obj2)
        {
            //return !obj1.Value.Equals(obj2.Value);
            return !obj1.x.Equals(obj2.x) || !obj1.y.Equals(obj2.y) || !obj1.z.Equals(obj2.z) || !obj1.w.Equals(obj2.w);
        }

        public bool Equals(Hash128 obj) { return x.Equals(obj.x) && y.Equals(obj.y) && z.Equals(obj.z) && w.Equals(obj.w); }

        public override bool Equals(object obj) { return obj is Hash128 other && Equals(other); }

        public static bool operator <(Hash128 a, Hash128 b)
        {
            if (a.w != b.w)
                return a.w < b.w;
            if (a.z != b.z)
                return a.z < b.z;
            if (a.y != b.y)
                return a.y < b.y;
            return a.x < b.x;
        }

        public static bool operator >(Hash128 a, Hash128 b)
        {
            if (a.w != b.w)
                return a.w > b.w;
            if (a.z != b.z)
                return a.z > b.z;
            if (a.y != b.y)
                return a.y > b.y;
            return a.x > b.x;
        }

        public int CompareTo(Hash128 other)
        {
            if (w != other.w)
                return w < other.w ? -1 : 1;
            if (z != other.z)
                return z < other.z ? -1 : 1;
            if (y != other.y)
                return y < other.y ? -1 : 1;
            if (x != other.x)
                return x < other.x ? -1 : 1;
            return 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Value.GetHashCode();

        public bool IsValid => Value.x != default || Value.y != default || Value.z != default || Value.w != default;

#if UNITY_EDITOR
        public static unsafe implicit operator Hash128(UnityEditor.GUID guid) => *(Hash128*) &guid;
        public static unsafe implicit operator UnityEditor.GUID(Hash128 guid) => *(UnityEditor.GUID*) &guid;
#endif

#if UNITY_2019_1_OR_NEWER
        public static unsafe implicit operator Hash128(UnityEngine.Hash128 guid) => *(Hash128*) &guid;
        public static unsafe implicit operator UnityEngine.Hash128(Hash128 guid) => *(UnityEngine.Hash128*) &guid;
#endif
    }
}