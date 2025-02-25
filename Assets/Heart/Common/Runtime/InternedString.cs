using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Pancake.Common
{
    /// <summary>
    /// A <see cref="string"/> wrapper which allows fast reference equality checks and dictionary usage
    /// by ensuring that users of identical strings are given the same <see cref="InternedString"/>
    /// instead of needing to compare each character in the strings.
    /// </summary>
    /// <remarks>
    /// Rather than a constructor, instances of this class are acquired via <see cref="Get(string)"/>
    /// or via implicit conversion from <see cref="string"/> (which calls the same method).
    /// <para></para>
    /// this implementation is case-sensitive and treats <c>null</c> and <c>""</c> as not equal.
    /// It's also a class to allow usage as a key in a dictionary keyed by <see cref="object"/> without boxing.
    /// <para></para>
    /// <strong>Example:</strong>
    /// <code>
    /// public static readonly InternedString str = "My String";
    /// </code>
    /// </remarks>
    public class InternedString : IComparable<InternedString>, IConvertable<string>
    {
        public readonly string source;

        private static readonly Dictionary<string, InternedString> Container = new();

        /// <summary>Returns a <see cref="InternedString"/> containing the `value`.</summary>
        /// <remarks>
        /// The returned result is cached and the same one will be
        /// returned each time this method is called with the same `value`.
        /// <para></para>
        /// Returns <c>null</c> if the `value` is <c>null</c>.
        /// <para></para>
        /// The `value` is case-sensitive.
        /// </remarks>
        public static InternedString Get(string value)
        {
            if (value is null) return null;

            // This system could be made case-insensitive based on a static bool.
            // If true, convert the value to lower case for the dictionary key but still reference the original.
            // When changing the setting, rebuild the dictionary with the appropriate keys.
            if (!Container.TryGetValue(value, out var result)) Container.Add(value, result = new InternedString(value));

            return result;
        }

        /// <summary>Creates a new array of <see cref="InternedString"/>s to the `values`.</summary>
        public static InternedString[] Get(params string[] values)
        {
            if (values == null) return null;

            if (values.Length == 0) return Array.Empty<InternedString>();

            var results = new InternedString[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                results[i] = values[i];
            }

            return results;
        }

        /// <summary>Returns a <see cref="InternedString"/> containing the `value` if one has already been created.</summary>
        /// <remarks>The `value` is case sensitive.</remarks>
        public static bool TryGet(string value, out InternedString result)
        {
            if (value is not null && Container.TryGetValue(value, out result)) return true;

            result = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private InternedString(string value) => source = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator InternedString(string value) => Get(value);

        /// <summary>
        /// Returns a new <see cref="InternedString"/> which will not be shared by regular calls to
        /// <see cref="Get(string)"/>.
        /// </summary>
        /// <remarks>
        /// This means the reference will never be equal to others
        /// even if they contain the same <see cref="String"/>.
        /// </remarks>
        internal static InternedString Unique(string value) => new(value);

        string IConvertable<string>.Convert() => source;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => source;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator string(InternedString value) => value?.source;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(InternedString other) => source.CompareTo(other?.source);
    }
}