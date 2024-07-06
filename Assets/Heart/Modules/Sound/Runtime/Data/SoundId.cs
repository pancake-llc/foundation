using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pancake.Sound
{
    [Serializable]
    public struct SoundId : IEquatable<SoundId>, IComparable<SoundId>, IEqualityComparer<SoundId>, IComparer<SoundId>
    {
        public int id;
#if UNITY_EDITOR
        [SerializeField] private ScriptableObject sourceAsset;
#endif

        public SoundId(int id)
            : this()
        {
            this.id = id;
        }

        public static SoundId Invalid => new SoundId(-1);
        public override string ToString() => id.ToString();
        public override bool Equals(object obj) => obj is SoundId soundId && Equals(soundId);
        public override int GetHashCode() => id;
        public bool Equals(SoundId other) => other.id == id;
        public bool Equals(SoundId x, SoundId y) => x.id == y.id;
        public int GetHashCode(SoundId obj) => obj.id;
        public int CompareTo(SoundId other) => other.id.CompareTo(id);
        public int Compare(SoundId x, SoundId y) => x.id.CompareTo(y.id);
        public static implicit operator int(SoundId soundId) => soundId.id;
        public static implicit operator SoundId(int id) => new(id);
    }
}