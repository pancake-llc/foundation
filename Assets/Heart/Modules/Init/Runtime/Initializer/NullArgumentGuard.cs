using System;
using System.Runtime.CompilerServices;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init
{
    /// <summary>
    /// Specifies how an <see cref="Initializer"/> should guard against null arguments.
    /// </summary>
    [Flags]
    public enum NullArgumentGuard
    {
        /// <summary>
        /// Allow <see langword="null"/> arguments to be present in Edit Mode and passed at runtime.
        /// </summary>
		None = _0,

        /// <summary>
        /// Warn about <see langword="null"/> arguments in Edit Mode.
        /// <para>
        /// Note that validation in Edit Mode might not give accurate results if
        /// some of the arguments only become available at runtime.
        /// </para>
        /// </summary>
		EditModeWarning = _1,

        /// <summary>
        /// Throw an exception if <see langword="null"/> arguments are detected at runtime.
        /// <para>
        /// This validation takes place just before the arguments are injected to the client.
        /// </para>
        /// </summary>
		RuntimeException = _2,

        /// <summary>
        /// Warn about <see langword="null"/> arguments detected on prefab assets.
        /// <para>
        /// Note that this option only affects prefab assets; prefab instances in scenes will
        /// still give warnings about <see langword="null"/> arguments in Edit Mode if the
        /// <see cref="EditModeWarning"/> flag is enabled.
        /// </para>
        /// </summary>
        EnabledForPrefabs = _3
    }

    /// <summary>
    /// Extension methods for <see cref="NullArgumentGuard"/>.
    /// </summary>
    public static class NullArgumentGuardExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsEnabled(this NullArgumentGuard @this, NullArgumentGuard flag) => (@this & flag) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisabled(this NullArgumentGuard @this, NullArgumentGuard flag) => (@this & flag) == 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NullArgumentGuard WithFlagToggled(this NullArgumentGuard @this, NullArgumentGuard flag) => (@this & flag) != 0 ? (@this & ~flag) : (@this | flag);
	}
}