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
		private static readonly object threadLock = new();
		private static bool? inPlayMode;
		private static bool exitingPlayMode;
		private static event Action ExitingPlayMode;

		/// <summary>
		/// Provides thread safe alternatives to some static properties of <see cref="UnityEngine.Application"/>.
		/// </summary>
		public static class Application
		{
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
			/// Gets a value indicating whether the Editor is in Play Mode or not.
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a thread safe operation and can be used instead of
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
						if(inPlayMode is bool result)
						{
							return result;
						}

						#if DEV_MODE
						Debug.LogWarning("ThreadSafe.Application.IsPlaying called, but unable to determine correct value safely at this time. Returning default value 'False'.");
						#endif

						return false;
					}
				}

				internal set
				{
					lock(threadLock)
					{
						inPlayMode = value;
					}
				}
			}

			/// <summary>
			/// Gets a value indicating whether the Editor is in Play Mode or not.
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a thread safe operation and can be used instead of
			/// <see cref="UnityEngine.Application.isPlaying"/> in threaded contexts such as in the constructor
			/// of a <see cref="Component"/> or during <see cref="ISerializationCallbackReceiver"/> events.
			/// </para>
			/// </summary>
			/// <param name="context"> The context from which a method is being called. </param>
			/// <param name="isPlaying">
			/// When this method returns, contains a value indicating whether or not we are currently in play mode,
			/// if it was possible to determine this; otherwise, <see langword="false"/>. </param>
			/// <returns>
			/// <see langword="true"/> if was able to determine if application is playing or not; otherwise, <see langword="false"/>.
			/// </returns>
			internal static bool TryGetIsPlaying(Context context, out bool isPlaying)
			{
				if(context.Is(Context.EditMode))
				{
					#if DEV_MODE
					Debug.Assert(!context.IsUnitySafeContext() || !UnityEngine.Application.isPlaying);
					#endif
					isPlaying = false;
					return true;
				}

				if(context.Is(Context.Runtime))
				{
					#if DEV_MODE
					Debug.Assert(!context.IsUnitySafeContext() || UnityEngine.Application.isPlaying);
					#endif
					isPlaying = true;
					return true;
				}

				if(context.IsUnitySafeContext())
				{
					isPlaying = UnityEngine.Application.isPlaying;
					return true;
				}

				lock(threadLock)
				{
					if(inPlayMode is bool result)
					{
						isPlaying = result;
						return true;
					}

					isPlaying = false;
					return false;
				}
			}

			/// <summary>
			/// Gets a value indicating whether the Editor is currently in the process of existing play mode or not.
			/// <para>
			/// The value of this property is updated during every <see cref="EditorApplication.playModeStateChanged"/>
			/// event to reflect the current state of the Editor.
			/// </para>
			/// <para>
			/// Reading the value of this property is a thread safe operation and can be used instead of
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
					}
					break;
				case PlayModeStateChange.ExitingEditMode:
					lock(threadLock)
					{
						inPlayMode = false;
						exitingPlayMode = false;
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