using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Sisus.Init
{
    /// <summary>
    /// Extension methods for <see cref="IWrapper"/> objects. 
    /// </summary>
    public static class WrapperExtensions
    {
        public static bool IsNull([AllowNull] this IWrapper wrapper) => (bool)(Object)wrapper == false;

        /// <summary>
        /// Destroys this wrapper using <see cref="Object.Destroy(Object)"/>.
        /// <para>
        /// If the wrapped object implements <see cref="System.IDisposable"/>, then <see cref="System.IDisposable.Dispose"/> will be called on it.
        /// </para>
        /// </summary>
        /// <param name="wrapper"> The wrapper to destroy. </param>
        public static void Destroy([DisallowNull] this IWrapper wrapper) => Object.Destroy((Object)wrapper);
    }
}