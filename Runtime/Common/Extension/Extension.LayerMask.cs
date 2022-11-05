using System.Runtime.CompilerServices;

namespace Pancake
{
    using UnityEngine;

    public static partial class C
    {
        /// <summary>
        /// Returns bool if layer is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool Contains(this LayerMask mask, int layer)
        {
            foreach (int index in mask.IterateBitIndices())
            {
                if (index == layer) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if gameObject is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="gameobject"></param>
        /// <returns></returns>
        [MethodImpl]
        public static bool Contains(this LayerMask mask, GameObject gameobject) => mask.Contains(gameobject.layer);


        public static BitIterator IterateBitIndices(this int aMask) { return new BitIterator(aMask); }
        public static BitIterator IterateBitIndices(this LayerMask aMask) { return new BitIterator(aMask); }

        public struct BitIterator
        {
            private ulong _mMask;
            private int _mIndex;

            public BitIterator(ulong aMask)
            {
                _mMask = aMask;
                _mIndex = -1;
            }

            public BitIterator(long aMask)
                : this((ulong) aMask)
            {
            }

            public BitIterator(uint aMask)
                : this((ulong) aMask)
            {
            }

            public BitIterator(int aMask)
                : this((ulong) aMask)
            {
            }

            public BitIterator(LayerMask aMask)
                : this((ulong) aMask.value)
            {
            }

            public BitIterator GetEnumerator() => this;
            public int Current => _mIndex;

            public bool MoveNext()
            {
                if (_mMask == 0)
                    return false;
                while ((_mMask & 1) == 0)
                {
                    _mMask >>= 1;
                    ++_mIndex;
                }

                _mMask >>= 1;
                return ++_mIndex < 64;
            }
        }
    }
}