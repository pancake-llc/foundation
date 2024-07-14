#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Sisus.Init.EditorOnly
{
	/// <summary>
	/// Provides thread safe alternatives to some of Unity's static properties.
	/// </summary>
	public static class ThreadSafe
	{
		private static readonly object threadLock = new object();
		private static bool inPlayMode = false;
		private static bool exitingPlayMode = false;
		private static event Action ExitingEditMode;
		private static event Action EnteredEditMode;
		private static event Action ExitingPlayMode;

		/// <summary>
		/// Provides thread safe alternatives to some static properties of <see cref="UnityEngine.Application"/>.
		/// </summary>
		public static class Application
		{
			/// <summary>
			/// Event that that gets invoked when the Editor begins the process of exiting edit mode.
			/// <para>
			/// Subscribing to or unsubscribing from this event is a thread safe operation and can be
			/// used instead of <see cref="EditorApplication.playModeStateChanged"/> for convenience
			/// to unsubscribe static event listeners.
			/// </para>
			/// </summary>
			public static event Action ExitingEditMode
			{
				add => ThreadSafe.ExitingEditMode += value;
				remove => ThreadSafe.ExitingEditMode -= value;
			}

			/// <summary>
			/// Event that that gets invoked when the Editor begins the process of existing play mode.
			/// <para>
			/// Subscribing to or unsubscribing from this event is a thread safe operation and can be
			/// used instead of <see cref="EditorApplication.playModeStateChanged"/> for convenience
			/// to unsubscribe static event listeners.
			/// </para>
			/// </summary>
			public static event Action ExitingPlayMode
			{
				add => ThreadSafe.ExitingPlayMode += value;
				remove => ThreadSafe.ExitingPlayMode -= value;
			}

			/// <summary>
			/// Event that that gets invoked when the Editor has finished the process of entering edit mode.
			/// <para>
			/// Subscribing to or unsubscribing from this event is a thread safe operation and can be
			/// used instead of <see cref="EditorApplication.playModeStateChanged"/> for convenience
			/// to unsubscribe static event listeners.
			/// </para>
			/// </summary>
			public static event Action EnteredEditMode
			{
				add => ThreadSafe.EnteredEditMode += value;
				remove => ThreadSafe.EnteredEditMode -= value;
			}

			/// <summary>
			/// Gets a value indicating whether or not the Editor is in Play Mode or not.
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a a thread safe operation and can be used instead of
			/// <see cref="UnityEngine.Application.isPlaying"/> in threaded contexts such as in the constructor
			/// of a <see cref="Component"/> or during <see cref="ISerializationCallbackReceiver"/> events.
			/// </para>
			/// </summary>
			public static bool IsPlaying
			{
				get
				{
					lock(threadLock)
					{
						return inPlayMode;
					}
				}
			}

			/// <summary>
			/// Gets a value indicating whether or not the Editor is currently in the process of existing play mode or not.
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a a thread safe operation and can be used instead of
			/// <see cref="UnityEngine.Application.isPlaying"/> in threaded contexts such as in the constructor
			/// of a <see cref="Component"/> or during <see cref="ISerializationCallbackReceiver"/> events.
			/// </para>
			/// </summary>
			public static bool IsExitingPlayMode
			{
				get
				{
					lock(threadLock)
					{
						return exitingPlayMode;
					}
				}
			}

			/// <summary>
			/// Gets a value indicating whether or not the Editor is currently in edit mode or in the process of
			/// existing Play Mode or not.
			/// <para>
			/// When the Editor is in Edit Mode this property always returns <see langword="true"/>.
			/// </para>
			/// <para>
			/// When the Editor is in Play Mode this property returns <see langword="false"/> except for when the
			/// Editor is in the process of transitioning from Play Mode to Edit Mode.
			/// </para>
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a a thread safe operation and can be used instead of
			/// <see cref="UnityEngine.Application.isPlaying"/> in threaded contexts such as in the constructor
			/// of a <see cref="Component"/> or during <see cref="ISerializationCallbackReceiver"/> events.
			/// </para>
			/// </summary>
			public static bool IsInEditModeOrExitingPlayMode
			{
				get
				{
					lock(threadLock)
					{
						return !inPlayMode || exitingPlayMode;
					}
				}
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void OnEnterPlayMode()
		{
			lock(threadLock)
			{
				inPlayMode = true;
				exitingPlayMode = false;
			}

			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private static void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			switch(state)
			{
				case PlayModeStateChange.EnteredEditMode:
					lock(threadLock)
					{
						inPlayMode = false;
						exitingPlayMode = false;
						EnteredEditMode?.Invoke();
					}
					break;
				case PlayModeStateChange.ExitingEditMode:
					lock(threadLock)
					{
						inPlayMode = false;
						exitingPlayMode = false;
						ExitingEditMode?.Invoke();
					}
					break;
				case PlayModeStateChange.EnteredPlayMode:
					lock(threadLock)
					{
						inPlayMode = true;
						exitingPlayMode = false;
					}
					break;
				case PlayModeStateChange.ExitingPlayMode:
					lock(threadLock)
					{
						inPlayMode = true;
						exitingPlayMode = true;
						ExitingPlayMode?.Invoke();
					}
					break;
			}
		}
	}
}
#endif