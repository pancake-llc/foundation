#if UNITY_EDITOR
using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Sisus.ComponentNames.Editor
{
	/// <summary>
	/// Specifies the different options that can be used when applying modifications.
	/// </summary>
	[Flags]
	internal enum ModifyOptions
	{
		/// <summary>
		/// Changes are applied after a delay.
		/// <para>
		/// Changes can be undone.
		/// </para>
		/// <para>
		/// The object that holds the custom name and tooltip for the component is be removed,
		/// if the component no longer has any name or tooltip override after the changes have been applied.
		/// </para>
		/// </summary>
		Defaults = 0,

		/// <summary>
		/// Changes are applied immediately.
		/// </summary>
		Immediate = 1,

		/// <summary>
		/// Changes can not be undone.
		/// </summary>
		NonUndoable = 2,

		/// <summary>
		/// The object that holds the custom name and tooltip for the component should not be removed, even if
		/// the component no longer has any name or tooltip override after the changes have been applied.
		/// <para>
		/// Use this option if you are using <see cref="Immediate"/> mode, and you are going to be making further
		/// changes to the name or tooltip of the component after these changes.
		/// </para>
		/// </summary>
		DisallowRemoveNameContainer = 4,

		/// <summary>
		/// The object that holds the custom name and tooltip for the component should not be affected by the operation.
		/// </summary>
		DontUpdateNameContainer = 8
	}

	internal static class ModifyOptionsExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsSet(this ModifyOptions options, ModifyOptions flag) => (options & flag) != 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDelayed(this ModifyOptions options) => !options.IsSet(ModifyOptions.Immediate);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsUndoable(this ModifyOptions options) => !options.IsSet(ModifyOptions.NonUndoable);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsRemovingNameContainerAllowed(this ModifyOptions options) => !options.IsSet(ModifyOptions.DisallowRemoveNameContainer) && options.IsUpdatingContainerAllowed();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsUpdatingContainerAllowed(this ModifyOptions options) => !options.IsSet(ModifyOptions.DontUpdateNameContainer);

		[Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ModifyOptions Immediately(this ModifyOptions options) => options | ModifyOptions.Immediate;
	}
}
#endif