using System;
using System.Diagnostics.Contracts;
using UnityEngine;

namespace Sisus.Init
{
	/// <summary>
	/// Specifies different contexts of interest from which methods can be called during initialization.
	/// <para>
	/// Used to identify the initialization phase during which a client is <see cref="InitArgs.TryGet">trying to get arguments</see>
	/// that have been <see cref="InitArgs.Set">provided</see> for it. Certain functionality of methods can be available
	/// only in particular contexts, sometimes for thread safety reasons, and other times because it is desired by-design.
	/// </para>
	/// </summary>
	[Flags]
	public enum Context
	{
		/// <summary>
		/// The method can be getting called from a background thread; not a thread safe context.
		/// <para>
		/// Whether the method is getting called in edit mode or at runtime is undetermined.
		/// </para>
		/// </summary>
		Default = 0,

		#region Flags
		/// <summary>
		/// The method is for certain getting called from the main thread.
		/// <para>
		/// A thread safe context.
		/// </para>
		/// </summary>
		MainThread = 1,

		/// <summary>
		/// The method is for certain getting called in edit mode.
		/// </summary>
		/// <seealso cref="InitInEditModeAttribute"/>
		EditMode = 2,

		/// <summary>
		/// The method is for certain getting called at runtime;
		/// either in a build or in Play Mode in the editor.
		/// </summary>
		Runtime = 4,
		#endregion

		#region Initialization Contexts
		/// <summary>
		/// The method is getting called from a constructor.
		/// <para>
		/// Not a thread safe context.
		/// </para>
		/// <para>
		/// Maybe edit mode.
		/// </para>
		/// </summary>
		/// <seealso cref="ConstructorBehaviour{TArgument}"/>
		Constructor = Default,

		/// <summary>
		/// The method is getting called during the <see cref="ISerializationCallbackReceiver.OnAfterDeserialize"/> event.
		/// <para>
		/// Not a thread safe context.
		/// </para>
		/// <para>
		/// Maybe edit mode.
		/// </para>
		/// </summary>
		OnAfterDeserialize = Default,

		/// <summary>
		/// The method is getting called during the Reset event.
		/// <para>
		/// A thread safe context.
		/// </para>
		/// <para>
		/// Is edit mode.
		/// </para>
		/// </summary>
		/// <seealso cref="InitOnResetAttribute"/>
		Reset = EditMode | MainThread,

		/// <summary>
		/// The method is getting called from the OnValidate event function in edit mode.
		/// <para>
		/// Not a thread safe context.
		/// </para>
		/// </summary>
		OnValidate = EditMode,

		/// <summary>
		/// The method is getting called from an Awake event function.
		/// <para>
		/// A thread safe context.
		/// </para>
		/// </summary>
		Awake = Runtime | MainThread,

		/// <summary>
		/// The method is getting called from an OnEnable event function.
		/// <para>
		/// A thread safe context.
		/// </para>
		/// </summary>
		OnEnable = Runtime | MainThread,

		/// <summary>
		/// The method is getting called from a Start event function.
		/// <para>
		/// A thread safe context.
		/// </para>
		/// </summary>
		Start = Runtime | MainThread
		#endregion
	}

	internal static class ContextExtensions
	{
		[Pure]
		public static bool Is(this Context context, Context flag) => (context & flag) == flag;

		[Pure]
		public static bool IsUnitySafeContext(this Context context) => Is(context, Context.MainThread);

		[Pure]
		public static bool IsMainThread(this Context context) => Is(context, Context.MainThread);

		[Pure]
		public static bool IsRuntime(this Context context)
		#if UNITY_EDITOR
			=> !context.IsEditMode();
		#else
			=> true;
		#endif

		[Pure]
		public static bool IsEditMode(this Context context)
		{
			#if UNITY_EDITOR
			if(context.Is(Context.EditMode))
			{
				return true;
			}

			if(context.Is(Context.Runtime))
			{
				return false;
			}

			if(context.Is(Context.MainThread))
			{
				return !Application.isPlaying;
			}

			return !EditorOnly.ThreadSafe.Application.IsPlaying;
			#else
			return false;
			#endif
		}

		public static bool TryDetermineIsRuntime(this Context context, out bool isRuntime)
		#if UNITY_EDITOR
			=> EditorOnly.ThreadSafe.Application.TryGetIsPlaying(context, out isRuntime);
		#else
		{
			isRuntime = true;
			return true;
		}
		#endif

		public static bool TryDetermineIsEditMode(this Context context, out bool isEditMode)
		#if UNITY_EDITOR
		{
			if(EditorOnly.ThreadSafe.Application.TryGetIsPlaying(context, out bool isPlayMode))
			{
				isEditMode = !isPlayMode;
				return true;
			}

			isEditMode = default;
			return false;
		}
		#else
		{
			isEditMode = false;
			return true;
		}
		#endif
	}
}