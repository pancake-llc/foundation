using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static Sisus.Init.FlagsValues;

namespace Sisus.Init.Internal
{
	/// <summary>
	/// Specifies the options for different Init arguments to target.
	/// </summary>
	[Flags]
	public enum Arguments
	{
		None = 0,
		First = _1,
		Second = _2,
		Third = _3,
		Fourth = _4,
		Fifth = _5,
		Sixth = _6,
		Seventh = _7,
		Eighth = _8,
		Ninth = _9,
		Tenth = _10,
		Eleventh = _11,
		Twelfth = _12
	}

	internal static class ArgumentExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
		public static bool ContainsFlag(this Arguments @this, Arguments flag) => (@this & flag) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining), Pure]
		public static Arguments WithFlag(this Arguments @this, Arguments flag, bool enabled)
			=> enabled ? (@this | flag) : (@this & ~flag);
	}
}