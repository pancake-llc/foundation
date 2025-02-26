using System;

namespace Pancake.Common
{
    public static partial class C
    {
        public static bool IsNullOrEmpty(this InternedString source) => source is null || source.data.Length == 0;

        /// <summary>
        /// Is the <see cref="InternedString.data"/> equal to the `other`
        /// when treating <c>""</c> as equal to <c>null</c>?
        /// </summary>
        public static bool EqualsWhereEmptyIsNull(this InternedString source, InternedString target)
        {
            if (source == target) return true;
            if (source == null) return target.data.Length == 0;
            if (source.data.Length == 0) return target == null;
            return false;
        }

        /// <summary>Creates a new array containing the <see cref="InternedString.data"/>s.</summary>
        public static string[] ToStrings(this InternedString[] sources)
        {
            if (sources == null) return null;
            if (sources.Length == 0) return Array.Empty<string>();

            var strings = new string[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                strings[i] = sources[i];
            }

            return strings;
        }
    }
}