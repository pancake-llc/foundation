using System;
using System.Runtime.CompilerServices;
using Pancake.Common;
using UnityEngine;

namespace Pancake
{
    [CreateAssetMenu(fileName = "string_key.asset", menuName = "Pancake/Scriptable/string key")]
    [EditorIcon("so_orange_key")]
    [Serializable]
    [Searchable]
    public class StringKey : ScriptableObject,
        IComparable<StringKey>,
        IConvertable<InternedString>,
        IConvertable<string>,
        IEquatable<StringKey>,
        IEquatable<InternedString>,
        IEquatable<string>,
        IKey
    {
        private InternedString _src;

        public InternedString Name
        {
#if UNITY_EDITOR
            get => _src = this ? name : "";
#else
            get => _src ??= name;
#endif

            set => _src = name = value;
        }

        public object Key => Name;

        #region equality

        public static int Compare(StringKey a, StringKey b) => a == b ? 0 : a ? a.CompareTo(b) : -1;

        public int CompareTo(StringKey other) => other ? Name.data.CompareTo(other.Name.data) : 1;

        public bool Equals(StringKey other) => other is not null && Name == other.Name;

        public bool Equals(InternedString other) => Name == other;

        public bool Equals(string other) => Name.data == other;

        public override bool Equals(object other)
        {
            return other switch
            {
                StringKey key => Equals(key),
                InternedString internedString => Equals(internedString),
                string str => Equals(str),
                _ => false
            };
        }

        public override int GetHashCode() => Name.GetHashCode();

        #endregion

        #region operator

        /// <summary>Are the <see cref="Name"/>s equal?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringKey a, StringKey b) => a is null ? b is null : a.Equals(b);

        /// <summary>Are the <see cref="Name"/>s not equal?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringKey a, StringKey b) => a is null ? b is not null : !a.Equals(b);

        /// <summary>Is the <see cref="Name"/> equal to `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringKey a, InternedString b) => a?.Name == b;

        /// <summary>Is the <see cref="Name"/> not equal to `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringKey a, InternedString b) => a?.Name != b;

        /// <summary>Is the <see cref="Name"/> equal to `a`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(InternedString a, StringKey b) => a == b?.Name;

        /// <summary>Is the <see cref="Name"/> not equal to `a`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(InternedString a, StringKey b) => a != b?.Name;

        /// <summary>Is the <see cref="Name"/> equal to `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(StringKey a, string b) => a?.Name.data == b;

        /// <summary>Is the <see cref="Name"/> not equal to `b`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(StringKey a, string b) => a?.Name.data != b;

        /// <summary>Is the <see cref="Name"/> equal to `a`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(string a, StringKey b) => b?.Name.data == a;

        /// <summary>Is the <see cref="Name"/> not equal to `a`?</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(string a, StringKey b) => b?.Name.data != a;

        #endregion

        #region convert

        InternedString IConvertable<InternedString>.Convert() => Name;

        string IConvertable<string>.Convert() => Name;

        public override string ToString() => Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator InternedString(StringKey value) => value?.Name;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(StringKey value) => value?.Name;

        /// <summary>Creates a new array containing the <see cref="Name"/>s.</summary>
        public static InternedString[] ToStringReferences(params StringKey[] keys)
        {
            if (keys == null) return null;

            if (keys.Length == 0) return Array.Empty<InternedString>();

            var strings = new InternedString[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                strings[i] = keys[i];
            }

            return strings;
        }

        /// <summary>Creates a new array containing the <see cref="Name"/>s.</summary>
        public static string[] ToStrings(params StringKey[] keys)
        {
            if (keys == null) return null;

            if (keys.Length == 0) return Array.Empty<string>();

            var strings = new string[keys.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                strings[i] = keys[i];
            }

            return strings;
        }

        #endregion

#if UNITY_EDITOR
        [SerializeField, TextArea(2, 25)] private string developerNote;
#endif
    }
}