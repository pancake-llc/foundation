#if UNITY_2023_1_OR_NEWER
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Sisus.Init.Internal
{
	internal sealed class Until : IUpdate
	{
		private static readonly IUpdate instance = new Until();
		private static readonly Queue<AwaitableCompletionSource> waitingForNextFrame = new();
		private static readonly Stack<AwaitableCompletionSource> completionSourcePool = new();

		/// <summary>
		/// Resumes execution on the Unity main thread, during an event where it's safe to use the Unity API.
		/// <para>
		/// An example use case is awaiting inside OnValidate until it's safe to access members like
		/// <see cref="Component.gameObject"/> and <see cref="Component.GetComponent{T}"/>.
		/// </para>
		/// </summary>
		/// <returns>
		/// Awaitable object that completes when it's safe to use the Unity API.
		/// </returns>
		public static Awaitable UnitySafeContext()
		{
			lock(instance)
			{
				if(completionSourcePool.TryPop(out var completionSource))
				{
					completionSource.Reset();
				}
				else
				{
					completionSource = new();
				}

				waitingForNextFrame.Enqueue(completionSource);

				return completionSource.Awaitable;
			}
		}

		#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		private static void InitInEditMode()
		{
			UnityEditor.EditorApplication.update -= OnUnitySafeContext;
			UnityEditor.EditorApplication.update += OnUnitySafeContext;
			EditorOnly.ThreadSafe.Application.IsPlaying = Application.isPlaying;
			OnUnitySafeContext();
		}
		#endif

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void InitAtRuntime()
		{
			Updater.Subscribe(instance);
			OnUnitySafeContext();
		}

		void IUpdate.Update(float deltaTime) => OnUnitySafeContext();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void OnUnitySafeContext()
		{
			lock(instance)
			{
				while(waitingForNextFrame.TryDequeue(out var completionSource))
				{
					completionSource.SetResult();
					completionSourcePool.Push(completionSource);
				}
			}
		}
	}
}
#endif